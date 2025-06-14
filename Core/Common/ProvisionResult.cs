namespace Core.Common;

public sealed record ProvisionResult(DateTime StartedAt, DateTime CompletedAt, bool Success, string? ExternalRef = null, string? Details = null);