using Domain.Mapping;

namespace Domain.Interfaces;

public interface IDataSource
{
    IEnumerable<IDictionary<string,string>> ReadAsync(CancellationToken cancellationToken);
    
    PluginDataModel? GetDataModel(string entity) => null;
}