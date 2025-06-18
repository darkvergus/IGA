using System.DirectoryServices.Protocols;
using System.Net;
using Core.Common;
using LDAPProvisioner.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Provisioning;
using Provisioning.Enums;
using Provisioning.Interfaces;

namespace LDAPProvisioner;

public sealed class LDAPProvisioner : IProvisioner
{
    public string ConnectorName => "LDAPProvisioner";

    private LDAPSettings? settings;
    private ILogger? logger;

    public void Initialize(IConfiguration cfg, ILogger log)
    {
        settings = cfg.Get<LDAPSettings>() ?? throw new InvalidOperationException("Settings not configured.");
        logger = log;
    }

    public async Task<ProvisionResult> RunAsync(ProvisioningCommand command, CancellationToken cancellationToken = default)
    {
        DateTime start = DateTime.UtcNow;

        try
        {
            using LdapConnection conn = Bind();

            switch (command.Operation)
            {
                case ProvisioningOperation.Create:
                    await CreateAsync(conn, command.ExternalId, command.Delta, cancellationToken);
                    break;
                case ProvisioningOperation.Update:
                    await UpdateAsync(conn, command.ExternalId, command.Delta, cancellationToken);
                    break;
                case ProvisioningOperation.Delete:
                    await DeleteAsync(conn, command.ExternalId, cancellationToken);
                    break;
            }
            
            string? sid = await ReadAttributeAsync(conn, command.ExternalId, "objectSid", cancellationToken);

            return new(start, DateTime.UtcNow, true, sid);
        }
        catch (Exception ex)
        {
            logger!.LogError(ex, $"LDAP {command.Operation} failed for {command.ExternalId}");

            return new(start, DateTime.UtcNow, false, null, ex.Message);
        }
    }

    private LdapConnection Bind()
    {
        LdapDirectoryIdentifier id = new(settings!.Host, settings.Port);
        NetworkCredential cred = new(settings.BindDn, settings.Password);

        LdapConnection connection = new(id)
        {
            AuthType = AuthType.Negotiate,
            Credential = cred,
            Timeout = TimeSpan.FromSeconds(30)
        };
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.SecureSocketLayer = settings.UseSsl;
        connection.Bind();

        return connection;
    }

    private static Task CreateAsync(LdapConnection connection, string dn, IReadOnlyDictionary<string, string>? attrs, CancellationToken _)
    {
        AddRequest request = new(dn);

        if (attrs is not null)
        {
            foreach ((string k, string value) in attrs)
            {
                request.Attributes.Add(new(k, value));
            }
        }

        connection.SendRequest(request);

        return Task.CompletedTask;
    }

    private static Task UpdateAsync(LdapConnection connection, string dn, IReadOnlyDictionary<string, string>? delta, CancellationToken _)
    {
        if (delta is null || delta.Count == 0)
        {
            return Task.CompletedTask;
        }

        ModifyRequest request = new(dn);

        foreach ((string attribute, string value) in delta)
        {
            DirectoryAttributeModification attributeModification = new()
            {
                Operation = DirectoryAttributeOperation.Replace,
                Name = attribute
            };
            attributeModification.Add(value);
            request.Modifications.Add(attributeModification);
        }

        connection.SendRequest(request);

        return Task.CompletedTask;
    }

    private static Task DeleteAsync(LdapConnection connection, string dn, CancellationToken _) => Task.FromResult(connection.SendRequest(new DeleteRequest(dn)));

    private static async Task<string?> ReadAttributeAsync(LdapConnection connection, string dn, string attribute, CancellationToken _)
    {
        SearchRequest request = new(dn, "(objectClass=*)", SearchScope.Base, attribute);
        SearchResponse? response = (SearchResponse)connection.SendRequest(request);
        SearchResultEntry? entry = response.Entries.Count > 0 ? response.Entries[0] : null;

        return entry == null ? null : entry.Attributes[attribute]?[0] is byte[] bytes ? Convert.ToBase64String(bytes) : entry.Attributes[attribute]?[0]?.ToString();
    }
}