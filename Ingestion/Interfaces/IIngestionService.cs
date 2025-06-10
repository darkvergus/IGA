namespace Ingestion.Interfaces;

public interface IIngestionService
{
    Task<ImportReport> IngestAsync(Type entityType, string sourceName, IEnumerable<IImportFilter> filters, CancellationToken cancellationToken = default);
}