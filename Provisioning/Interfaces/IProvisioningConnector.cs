namespace Provisioning.Interfaces;

/// <summary>
/// Contract for all outbound provisioning connectors.
/// </summary>
public interface IProvisioningConnector
{
    /// <summary>Unique name (e.g., "ldap", "scim", "sailpoint") used for DI.</summary>
    string Name { get; }

    /// <summary>
    /// Executes the operation and returns a result after performing a callback
    /// (e.g., reading LDAP attributes assigned by the directory).
    /// </summary>
    Task<ProvisioningResult> ExecuteAsync(ProvisioningCommand command, CancellationToken cancellation = default);
}