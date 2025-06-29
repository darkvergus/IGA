using Provisioning.Enums;

namespace Provisioning;

/// <summary>
/// Represents a provisioning request (create/update/delete) for a single account.
/// <para>
/// <b>ExternalId</b> carries the authoritative identifier inside the target
/// system (e.g., LDAP DN, SCIM Id), decoupling connectors from domain models.
/// </para>
/// </summary>
public sealed record ProvisioningCommand(ProvisioningOperation Operation, string ExternalId, IReadOnlyDictionary<string, string>? Delta = null);