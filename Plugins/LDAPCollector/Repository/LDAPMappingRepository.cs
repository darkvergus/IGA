using Ingestion.Interfaces;
using Ingestion.Mapping;

namespace LDAPCollector.Repository;

public sealed class LDAPMappingRepository : IMappingRepository
{
    private static readonly Dictionary<string,ImportMapping> Cache = MappingLoader.Load(typeof(LDAPMappingRepository).Assembly);
    
    public static ImportMapping? Get(string entity) => Cache.TryGetValue(entity, out ImportMapping? mapping) ? mapping : null;

    ImportMapping? IMappingRepository.Get(string entity) => Get(entity);

    public static void Register(string entity, ImportMapping mapping) => Cache[entity] = mapping;
}
