namespace Provisioning;

/// <summary>
/// Result returned by a connector after the callback phase.
/// </summary>
public sealed record ProvisioningResult(bool Success, string? ExternalId = null, string? ExternalSid = null, IReadOnlyDictionary<string, string>? AdditionalInfo = null, string? Error = null);