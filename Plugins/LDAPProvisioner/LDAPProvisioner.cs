using System.Collections.Concurrent;
using System.DirectoryServices.Protocols;
using System.Linq.Dynamic.Core;
using System.Net;
using Core.Common;
using Core.Dynamic;
using Database.Context;
using Domain.Mapping;
using Domain.Repository;
using LDAPProvisioner.Settings;
using Microsoft.EntityFrameworkCore;
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
    ILogger<LDAPProvisioner> log) : IProvisioner
{
    public string Name => "LDAPProvisioner";

    private LDAPSettings? settings;

    private readonly ConcurrentBag<LdapConnection> idleConnections = [];
    private const int MaxPoolSize = 10;

    public void Initialize(IConfiguration cfg, ILogger logger) =>
        settings = cfg.Get<LDAPSettings>() ?? throw new InvalidOperationException("LDAP settings not configured.");

    public async Task<ProvisionResult> RunAsync(ProvisioningCommand command, CancellationToken cancellationToken = default)
    {
        DateTime started = DateTime.UtcNow;

        if (command.Delta is null)
        {
            try
            {
                string entity = cfg["Entity"] ?? "identity";

                ImportMapping? importMapping = MappingRepository.Get(Name, entity);

                if (importMapping?.TargetEntityType == null)
                {
                    throw new InvalidOperationException("ImportMapping missing TargetEntityType.");
                }

                using IServiceScope scope = scopeFactory.CreateScope();
                IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                ProvisioningPipeline pipeline = new(db);

                Func<IQueryable, IQueryable>? filter = null;
                
                if (filter == null && !string.IsNullOrWhiteSpace(command.ExternalId))
                {
                    string pk = importMapping.PrimaryKeyProperty ?? "BusinessKey";
                    filter = queryable => queryable.Where($"{pk} == @0", command.ExternalId);
                }

                if (importMapping != null)
                {
                    await pipeline.RunAsync(mapping: importMapping, definitions: attributeDefinitions, provisioner: this, operation: command.Operation,
                        filter: filter, cancellationToken: cancellationToken);
                }

                return new(started, DateTime.UtcNow, true, ExternalRef: null, Details: "Pipeline completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "LDAP pipeline failed.");

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
                    await CreateAsync(connection, command.ExternalId, command.Delta!, cancellationToken);

                    break;
                case ProvisioningOperation.Update:
                    await UpdateAsync(connection, command.ExternalId, command.Delta!, cancellationToken);

                    break;
                case ProvisioningOperation.Delete:
                    await DeleteAsync(connection, command.ExternalId, cancellationToken);

                    break;
            }

            string? sid = await ReadAttributeAsync(connection, command.ExternalId, "objectSid", cancellationToken);

            return new(started, DateTime.UtcNow, true, ExternalRef: sid);
        }
        catch (LdapException ex)
        {
            log.LogError(ex, $"LDAP error {ex.ErrorCode}: {ex.Message}");

            return new(started, DateTime.UtcNow, false, null, ex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"LDAP {command.Operation} failed for {command.ExternalId}");

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

        return await BindAsync(settings!, log, cancellationToken);
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

            NetworkCredential cred = settings.AuthType.Equals("Negotiate", StringComparison.OrdinalIgnoreCase) || settings.AuthType.Equals("Ntlm", StringComparison.OrdinalIgnoreCase)
                ? new(settings.BindDn, settings.Password, settings.Domain) : new NetworkCredential(settings.BindDn, settings.Password);

            AuthType auth = Enum.TryParse(settings.AuthType, true, out AuthType parsed) ? parsed : AuthType.Basic;

            LdapConnection cn = new(id, cred, auth)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            cn.SessionOptions.ProtocolVersion = 3;
            cn.SessionOptions.SecureSocketLayer = settings.UseSsl;

            cn.Bind();
            log.LogDebug($"LDAP bind OK for {settings.BindDn}");

            return cn;
        }, cancellationToken);

    private static Task CreateAsync(LdapConnection connection, string dn, IReadOnlyDictionary<string, string> attributes, CancellationToken _)
    {
        AddRequest request = new(dn, new DirectoryAttribute("objectClass", "user"));

        foreach ((string key, string value) in attributes)
        {
            request.Attributes.Add(new(key, value));
        }

        connection.SendRequest(request);

        return Task.CompletedTask;
    }

    private static Task UpdateAsync(LdapConnection connection, string dn, IReadOnlyDictionary<string, string> delta, CancellationToken _)
    {
        if (delta.Count == 0)
        {
            return Task.CompletedTask;
        }

        ModifyRequest req = new(dn);

        foreach ((string key, string value) in delta)
        {
            DirectoryAttributeModification attributeModification = new()
            {
                Name = key,
                Operation = DirectoryAttributeOperation.Replace
            };

            attributeModification.Add(value);
            req.Modifications.Add(attributeModification);
        }

        connection.SendRequest(req);

        return Task.CompletedTask;
    }

    private static Task DeleteAsync(LdapConnection connection, string dn, CancellationToken _)
    {
        connection.SendRequest(new DeleteRequest(dn));

        return Task.CompletedTask;
    }

    private static Task<string?> ReadAttributeAsync(LdapConnection cn, string dn, string attr, CancellationToken _)
    {
        SearchRequest request = new(dn, "(objectClass=*)", SearchScope.Base, attr);
        SearchResponse? response = (SearchResponse)cn.SendRequest(request);
        SearchResultEntry? entry = response.Entries.Count > 0 ? response.Entries[0] : null;

        if (entry == null)
        {
            return Task.FromResult<string?>(null);
        }

        object? val = entry.Attributes[attr]?[0];

        return val switch
        {
            byte[] b => Task.FromResult<string?>(Convert.ToBase64String(b)),
            _ => Task.FromResult(val?.ToString())
        };
    }
}