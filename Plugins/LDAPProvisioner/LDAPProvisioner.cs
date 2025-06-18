using Core.Common;
using LDAPProvisioner.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Provisioning.Interfaces;

namespace LDAPProvisioner;

public sealed class LDAPProvisioner(IServiceProvider root) : IProvisioner
{
    public string ConnectorName => "LDAPProvisioner";

    private LDAPSettings? settings;
    private ILogger? logger;

    public void Initialize(IConfiguration cfg, ILogger log)
    {
        settings = cfg.Get<LDAPSettings>() ?? throw new InvalidOperationException("Settings not configured.");
        logger = log;
    }

    public async Task<ProvisionResult> ProvisionAsync(Entity<object> payload, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
