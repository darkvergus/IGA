namespace Ingestion.Interfaces;

/// <summary>
/// Provides mapping metadata and conversion logic between raw source rows and
/// domain entity instances. Concrete implementations (CSV, SAP, etc.) live in
/// their respective source projects.
/// </summary>
public interface IMappingResolver
{
    /// <param name="entityType">Concrete CLR type to be produced (e.g. typeof(Identity)).</param>
    /// <param name="mappingId">Identifier chosen in the UI / configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A compiled <see cref="IRowMapper"/> ready for fast row→entity conversion.</returns>
    Task<IRowMapper> ResolveAsync(Type entityType, Guid mappingId, CancellationToken cancellationToken = default);
}
