using Domain.Interfaces;
using Domain.Mapping;

namespace Domain.Repository;

public sealed class MappingRepository : IMappingRepository
{
    private static readonly Dictionary<string,ImportMapping> Cache = MappingLoader.Load(typeof(MappingRepository).Assembly);
    
    public static ImportMapping? Get(string plugin, string entity) => Cache.TryGetValue($"{plugin}:{entity}", out ImportMapping? mapping) ? mapping : null;

    ImportMapping? IMappingRepository.Get(string plugin, string entity) => Get(plugin, entity);

    public static void Register(string entity, ImportMapping mapping, string plugin) => Cache[$"{plugin}:{entity}"] = mapping;
}
