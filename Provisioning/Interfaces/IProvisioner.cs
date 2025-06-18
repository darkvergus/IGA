using Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Provisioning.Interfaces;

public interface IProvisioner
{
    string ConnectorName { get; }
    void Initialize(IConfiguration cfg, ILogger logger);
    Task<ProvisionResult> RunAsync(ProvisioningCommand command, CancellationToken cancellationToken = default);
}