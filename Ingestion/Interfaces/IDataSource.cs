namespace Ingestion.Interfaces;

public interface IDataSource
{
    IEnumerable<IDictionary<string, string>> ReadRecords();
}