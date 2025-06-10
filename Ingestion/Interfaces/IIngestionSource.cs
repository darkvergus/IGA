namespace Ingestion.Interfaces;

/// <summary>
/// Fetches records from an external system and converts them into instances of
/// the requested <paramref name="entityType"/>.
///
/// The contract purposely avoids a fixed enum so that callers can request any
/// domain type that derives from <c>Core.Domain.Entities.Entity&lt;TId&gt;</c> — or
/// indeed any POCO in the future (e.g. TextCodes).
/// </summary>
public interface IIngestionSource
{
    string Name { get; }

    IAsyncEnumerable<object> FetchAsync(Type entityType, CancellationToken cancellationToken = default);
}