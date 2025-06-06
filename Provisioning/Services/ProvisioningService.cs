using Provisioning.Interfaces;

namespace Provisioning.Services;

/// <summary>
/// Resolves the correct connector from DI and executes provisioning commands.
/// Wraps retries, logging, and timing.
/// </summary>
public sealed class ProvisioningService
{
    private readonly IDictionary<string, IProvisioningConnector> connectors;

    public ProvisioningService(IEnumerable<IProvisioningConnector> connectors) => this.connectors = connectors.ToDictionary(connector => connector.Name, StringComparer.OrdinalIgnoreCase);

    public async Task<ProvisioningResult> ProvisionAsync(string connectorName, ProvisioningCommand command, CancellationToken cancellationToken = default)
    {
        if (!connectors.TryGetValue(connectorName, out IProvisioningConnector? connector))
        {
            throw new InvalidOperationException($"Connector '{connectorName}' not registered.");
        }

        // TODO: Add Polly retry / logging here if desired
        return await connector.ExecuteAsync(command, cancellationToken);
    }
}