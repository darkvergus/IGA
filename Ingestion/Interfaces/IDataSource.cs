using Ingestion.Mapping;

namespace Ingestion.Interfaces;

public interface IDataSource
{
    IEnumerable<IDictionary<string,string>> ReadAsync(CancellationToken cancellationToken);
    
    PluginDataModel? GetDataModel(string entity) => null;
}