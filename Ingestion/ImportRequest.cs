namespace Ingestion;

public sealed record ImportRequest(Guid MappingId, IList<string> FilterIds, bool DryRun = false);