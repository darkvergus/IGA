using Domain.Interfaces;
using Domain.Mapping;

namespace CsvCollector.Repository;

public sealed class CsvMappingRepository : IMappingRepository
{
    private static readonly Dictionary<string,ImportMapping> Cache = MappingLoader.Load(typeof(CsvMappingRepository).Assembly);
    
    public static ImportMapping? Get(string entity) => Cache.TryGetValue(entity, out ImportMapping? mapping) ? mapping : null;

    ImportMapping? IMappingRepository.Get(string entity) => Get(entity);

    public static void Register(string entity, ImportMapping mapping) => Cache[entity] = mapping;
}
