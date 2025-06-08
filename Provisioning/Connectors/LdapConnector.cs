using Microsoft.Extensions.Options;
using Provisioning.Enums;
using Provisioning.Interfaces;
using Provisioning.Options;
using System.Data.Common;
using System.DirectoryServices.Protocols;
using System.Net;
using Core.Domain.Dynamic;

namespace Provisioning.Connectors;

/// <summary>
/// Simple LDAP connector that supports create / update / delete and reads the
/// resulting objectSid (or any additional attributes) in the callback phase.
/// </summary>
/// <summary>
/// Simple LDAP connector that supports create / update / delete and reads the
/// resulting objectSid (or any additional attributes) in the callback phase.
/// </summary>
public sealed class LdapConnector : IProvisioningConnector, IDisposable
{
    public const string SidAttribute = "objectSid";
    private readonly LdapConnection connection;

    public string Name => "ldap";

    public LdapConnector(IOptions<LdapOptions> opt) : this(opt.Value) { }

    public LdapConnector(LdapOptions opt)
    {
        NetworkCredential cred = new(opt.UserDn, opt.Password);

        connection = new LdapConnection(new LdapDirectoryIdentifier(opt.HostUrl, opt.Port))
        {
            AuthType = AuthType.Negotiate,
            Credential = cred,
            Timeout = TimeSpan.FromSeconds(30),
            SessionOptions =
            {
                ProtocolVersion = 3
            }
        };

        try
        {
            connection.Bind();
        }
        catch (LdapException ex)
        {
            Console.WriteLine($"LDAP code: {ex.ErrorCode}");
            Console.WriteLine($"Server msg: {ex.ServerErrorMessage}");
        }
    }

    public async Task<ProvisioningResult> ExecuteAsync(ProvisioningCommand cmd, CancellationToken cancellation = default)
    {
        string dn = cmd.ExternalId ?? throw new InvalidOperationException("Missing ExternalId / DN");

        try
        {
            switch (cmd.Operation)
            {
                case ProvisioningOperation.Create:
                    await CreateAsync(dn, cmd.Delta, cancellation);
                    break;
                case ProvisioningOperation.Update:
                    await UpdateAsync(dn, cmd.Delta, cancellation);
                    break;
                case ProvisioningOperation.Delete:
                    await DeleteAsync(dn, cancellation);
                    break;
            }

            // Callback: read SID and any attribute requested
            string? sid = await ReadAttributeAsync(dn, SidAttribute, cancellation);
            return new(true, dn, sid);
        }
        catch (Exception ex)
        {
            return new(false, dn, null, null, ex.Message);
        }
    }

    private Task CreateAsync(string dn, IReadOnlyDictionary<string, DynamicAttributeValue>? attrs, CancellationToken cancellationToken)
    {
        AddRequest request = new(dn);
        if (attrs is not null)
        {
            foreach ((string key, DynamicAttributeValue val) in attrs)
            {
                request.Attributes.Add(new(key, val.ToString()));
            }
        }
        connection.SendRequest(request);
        return Task.CompletedTask;
    }

    private Task UpdateAsync(string dn, IReadOnlyDictionary<string, DynamicAttributeValue>? delta, CancellationToken cancellationToken)
    {
        if (delta is null || delta.Count == 0)
        {
            return Task.CompletedTask;
        }

        ModifyRequest req = new(dn);
        foreach ((string attr, DynamicAttributeValue val) in delta)
        {
            DirectoryAttributeModification mod = new()
            {
                Operation = DirectoryAttributeOperation.Replace,
                Name = attr
            };
            mod.Add(val.ToString());
            req.Modifications.Add(mod);
        }
        connection.SendRequest(req);
        return Task.CompletedTask;
    }

    private Task DeleteAsync(string dn, CancellationToken cancellationToken)
    {
        connection.SendRequest(new DeleteRequest(dn));
        return Task.CompletedTask;
    }

    private async Task<string?> ReadAttributeAsync(string dn, string attr, CancellationToken cancellationToken)
    {
        SearchRequest req = new(dn, "(objectClass=*)", SearchScope.Base, attr);
        SearchResponse? res = (SearchResponse)connection.SendRequest(req);
        SearchResultEntry? entry = res.Entries.Count > 0 ? res.Entries[0] : null;
        if (entry == null)
        {
            return null;
        }

        byte[]? bytes = (byte[]?)entry.Attributes[attr]?[0];
        return bytes switch
        {
            null => null,
            _ => Convert.ToBase64String(bytes)
        };
    }

    public void Dispose() => connection.Dispose();
}
