namespace Core.Common;

public sealed record CollectionResult(DateTime StartedAt, DateTime CompletedAt, int RecordsCollected, bool Success, string? Details = null);