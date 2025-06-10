using Ingestion.Interfaces;

namespace Ingestion.Resolver;

// Simple no‑op fallback to unblock compilation until the real portal-driven
// implementation is available.
public sealed class PassthroughMappingResolver : IMappingResolver, IRowMapper
{
    public Task<IRowMapper> ResolveAsync(Type entityType, Guid mappingId, CancellationToken cancellationToken = default) => Task.FromResult<IRowMapper>(this);

    public object Convert(object rawRecord) => rawRecord;
}