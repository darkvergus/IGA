using Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Provisioning.Interfaces;

namespace Provisioning.Core;

public abstract class BaseProvisioner : IProvisioner
{
    protected IConfiguration Config { get; private set; } = default!;
    protected ILogger Logger { get; private set; } = default!;

    public abstract string Name { get; }

    public virtual void Initialize(IConfiguration cfg, ILogger logger) => (Config, Logger) = (cfg, logger);
    public abstract Task<ProvisionResult> RunAsync(ProvisioningCommand command, CancellationToken cancellationToken = default);
}