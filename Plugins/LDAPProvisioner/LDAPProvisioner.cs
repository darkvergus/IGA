using System.Collections.Concurrent;
using System.DirectoryServices.Protocols;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Principal;
using Core.Common;
using Core.Dynamic;
using Database.Context;
using Domain.Mapping;
using Domain.Repository;
using LDAPProvisioner.Extensions;
using LDAPProvisioner.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Provisioning;
using Provisioning.Enums;
using Provisioning.Interfaces;
using Provisioning.Pipeline;
using ZLinq;

namespace LDAPProvisioner;

public sealed class LDAPProvisioner(
    IEnumerable<DynamicAttributeDefinition> attributeDefinitions,
    IServiceScopeFactory scopeFactory,
    IConfiguration cfg,
    ILoggerFactory loggerFactory) : IProvisioner
{
    public string Name => "LDAPProvisioner";

    private LDAPSettings? settings;

    private readonly ILogger<LDAPProvisioner> logger = loggerFactory.CreateLogger<LDAPProvisioner>();
    private readonly ILogger<ProvisioningPipeline> pipelineLogger = loggerFactory.CreateLogger<ProvisioningPipeline>();

    private readonly ConcurrentBag<LdapConnection> idleConnections = [];
    private const int MaxPoolSize = 10;

    public void Initialize(IConfiguration cfg, ILogger logger)
    {
        settings = cfg.Get<LDAPSettings>() ?? throw new InvalidOperationException("LDAP settings not configured.");
        logger.LogInformation("LDAP settings initialized.");
    }

    public async Task<ProvisionResult> RunAsync(ProvisioningCommand command, CancellationToken cancellationToken = default)
    {
        DateTime started = DateTime.UtcNow;

        string entity = cfg["Entity"] ?? "identity";
        ImportMapping importMapping = MappingRepository.Get(Name, entity) ?? throw new InvalidOperationException($"No mapping found for {entity}");
        
        if (importMapping.TargetEntityType == null)
        {
            throw new InvalidOperationException("ImportMapping missing TargetEntityType.");
        }

        if (command.Delta is null)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                ProvisioningPipeline pipeline = new(db, pipelineLogger);

                Func<IQueryable, IQueryable>? filter = null;

                if (filter == null && !string.IsNullOrWhiteSpace(command.ExternalId))
                {
                    string keyProperty = importMapping.PrimaryKeyProperty ?? "BusinessKey";
                    filter = queryable => queryable.Where($"{keyProperty} == @0", command.ExternalId);
                }

                await pipeline.RunAsync(mapping: importMapping, definitions: attributeDefinitions, provisioner: this, operation: command.Operation,
                    filter: filter, cancellationToken: cancellationToken);

                return new(started, DateTime.UtcNow, true, ExternalRef: null, Details: "Pipeline completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LDAP pipeline failed.");

                return new(started, DateTime.UtcNow, false, ExternalRef: null, Details: ex.Message);
            }
        }

        LdapConnection? connection = null;

        try
        {
            connection = await AcquireAsync(cancellationToken);

            switch (command.Operation)
            {
                case ProvisioningOperation.Create:
                    if (settings != null)
                    {
                        await CreateWithMappingAsync(connection, settings.BaseDn, importMapping, command.Delta!, cancellationToken);
                    }

                    break;
                case ProvisioningOperation.Update:
                    if (settings != null)
                    {
                        await UpdateWithMappingAsync(connection, settings.BaseDn, importMapping, command.Delta!, cancellationToken);
                    }

                    break;
                case ProvisioningOperation.Delete:
                    if (settings != null)
                    {
                        await DeleteWithMappingAsync(connection, settings.BaseDn, importMapping, command.Delta!, cancellationToken);
                    }

                    break;
            }

            //string? sid = await ReadAttributeAsync(connection, command.ExternalId, "objectSid", cancellationToken);
            //
            //            return new(started, DateTime.UtcNow, true, ExternalRef: sid);
            return new(started, DateTime.UtcNow, true, ExternalRef: null, Details: "Pipeline completed");
        }
        catch (LdapException ex)
        {
            logger.LogError(ex, $"LDAP error {ex.ErrorCode}: {ex.Message}");

            return new(started, DateTime.UtcNow, false, null, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"LDAP {command.Operation} failed for {command.ExternalId}");

            return new(started, DateTime.UtcNow, false, ExternalRef: null, Details: ex.Message);
        }
        finally
        {
            if (connection is not null)
            {
                Release(connection);
            }
        }
    }

    private async Task<LdapConnection> AcquireAsync(CancellationToken cancellationToken)
    {
        if (idleConnections.TryTake(out LdapConnection? connection))
        {
            if (IsAlive(connection))
            {
                return connection;
            }

            DisposeSafely(connection);
        }

        return await BindAsync(settings!, logger, cancellationToken);
    }

    private void Release(LdapConnection connection)
    {
        if (!IsAlive(connection) || idleConnections.Count >= MaxPoolSize)
        {
            DisposeSafely(connection);
        }
        else
        {
            idleConnections.Add(connection);
        }
    }

    private static bool IsAlive(LdapConnection connection)
    {
        SearchRequest request = new("", "(objectClass=*)", SearchScope.Base, "namingContexts");

        try
        {
            connection.SendRequest(request);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DisposeSafely(LdapConnection connection)
    {
        try
        {
            connection.Dispose();
        }
        catch
        {
            /* ignore */
        }
    }

    private static Task<LdapConnection> BindAsync(LDAPSettings settings, ILogger log, CancellationToken cancellationToken)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            LdapDirectoryIdentifier id = new(settings.Host, settings.Port);

            NetworkCredential cred = settings.AuthType.Equals("Negotiate", StringComparison.OrdinalIgnoreCase) ||
                                     settings.AuthType.Equals("Ntlm", StringComparison.OrdinalIgnoreCase)
                ? new(settings.BindDn, settings.Password, settings.Domain)
                : new NetworkCredential(settings.BindDn, settings.Password);

            AuthType auth = Enum.TryParse(settings.AuthType, true, out AuthType parsed) ? parsed : AuthType.Basic;

            LdapConnection connection = new(id, cred, auth)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            connection.SessionOptions.ProtocolVersion = 3;
            
            connection.SessionOptions.Signing = true;
            connection.SessionOptions.Sealing = true;
            
            connection.Bind();
            log.LogDebug($"LDAP bind OK for {settings.BindDn}");

            return connection;
        }, cancellationToken);

    /// <summary>
    /// Creates a new entry under BaseDn using your ImportMapping.
    /// </summary>
    private static Task CreateWithMappingAsync(LdapConnection connection, string baseDn, ImportMapping importMapping, IReadOnlyDictionary<string, string> delta,
        CancellationToken _)
    {
        string keyProperty = importMapping.PrimaryKeyProperty ?? throw new InvalidOperationException("No PK set");

        ImportMappingItem importMappingItem = importMapping.FieldMappings.AsValueEnumerable()
            .First(mappingItem => mappingItem.SourceFieldName.Equals(keyProperty, StringComparison.OrdinalIgnoreCase));

        string fieldName = importMappingItem.TargetFieldName;
        
        if (!delta.TryGetValue(fieldName, out string? value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing RDN value '{fieldName}' in delta.");
        }

        string dn = $"CN={value},{baseDn}";

        AddRequest request = new(dn, new DirectoryAttribute("objectClass", "user"));

        foreach (ImportMappingItem field in importMapping.FieldMappings)
        {
            if (!delta.TryGetValue(field.TargetFieldName, out string? raw) || string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }
            
            if (!LDAPExtensions.TryConvertLdapValue(field.TargetFieldName, raw, out object ldapVal))
            {
                throw new InvalidOperationException($"Value '{raw}' is not valid for {field.TargetFieldName}");
            }
            
            request.Attributes.Add(new(field.TargetFieldName, ldapVal));
        }

        connection.SendRequest(request);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Modifies an existing entry under BaseDn using your ImportMapping.
    /// </summary>
    private static Task UpdateWithMappingAsync(LdapConnection connection, string baseDn, ImportMapping importMapping, IReadOnlyDictionary<string, string> delta,
        CancellationToken _)
    {
        string keyProperty = importMapping.PrimaryKeyProperty ?? throw new InvalidOperationException("No PK set");

        ImportMappingItem importMappingItem = importMapping.FieldMappings.AsValueEnumerable()
            .First(mappingItem => mappingItem.SourceFieldName.Equals(keyProperty, StringComparison.OrdinalIgnoreCase));

        if (!delta.TryGetValue(importMappingItem.SourceFieldName, out string? value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing PK value '{importMappingItem.SourceFieldName}'");
        }

        string dn = $"CN={value},{baseDn}";

        ModifyRequest req = new(dn);

        foreach (ImportMappingItem field in importMapping.FieldMappings)
        {
            if (delta.TryGetValue(field.SourceFieldName, out string? v) && !string.IsNullOrWhiteSpace(v))
            {
                DirectoryAttributeModification attributeModification = new()
                {
                    Name = field.TargetFieldName,
                    Operation = DirectoryAttributeOperation.Replace
                };

                attributeModification.Add(v);
                req.Modifications.Add(attributeModification);
            }
        }

        if (req.Modifications.Count > 0)
        {
            connection.SendRequest(req);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entry under BaseDn based on your ImportMapping’s PK.
    /// </summary>
    private static Task DeleteWithMappingAsync(LdapConnection connection, string baseDn, ImportMapping importMapping, IReadOnlyDictionary<string,string> delta,
        CancellationToken _)
    {
        string keyProperty = importMapping.PrimaryKeyProperty ?? throw new InvalidOperationException("No PK set");
        ImportMappingItem importMappingItem = importMapping.FieldMappings.AsValueEnumerable().First(mappingItem => mappingItem.SourceFieldName.Equals(keyProperty, StringComparison.OrdinalIgnoreCase));

        if (!delta.TryGetValue(importMappingItem.SourceFieldName, out string? value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing PK value '{importMappingItem.SourceFieldName}'");
        }

        string dn = $"CN={value},{baseDn}";

        connection.SendRequest(new DeleteRequest(dn));
        return Task.CompletedTask;
    }


    private static Task<string?> ReadAttributeAsync(LdapConnection connection, string dn, string attribute, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            SearchRequest request = new(dn, "(objectClass=*)", SearchScope.Base, attribute);
            SearchResponse? response = (SearchResponse)connection.SendRequest(request);
            SearchResultEntry? entry = response.Entries.Count > 0 ? response.Entries[0] : null;

            if (entry == null)
            {
                return null;
            }

            object? raw = entry.Attributes[attribute]?[0];

            if (raw is byte[] b)
            {
                return new SecurityIdentifier(b, 0).Value;
            }

            return raw?.ToString();
        }, cancellationToken);
    }
}