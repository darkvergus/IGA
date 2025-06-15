using System.Reflection;
using ZLinq;

namespace Ingestion.Mapping;

public static class MappingLoader
{
    public static Dictionary<string, ImportMapping> Load(Assembly pluginAssembly)
    {
        string dllDir = Path.GetDirectoryName(pluginAssembly.Location)!;
        string assemblyNm = Path.GetFileNameWithoutExtension(pluginAssembly.Location);
        string path = Path.Combine(dllDir, assemblyNm, "Mappings");

        Dictionary<string, ImportMapping> cache = new(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(path))
        {
            return cache;
        }

        foreach (string file in Directory.EnumerateFiles(path, "*.mapping.xml"))
        {
            string entityName = Path.GetFileNameWithoutExtension(file).Replace(".mapping", "", StringComparison.OrdinalIgnoreCase);
            ImportMapping? map = XmlMappingLoader.Load(Path.Combine(dllDir, assemblyNm), entityName);

            if (map == null)
            {
                continue;
            }

            Type? clr = Type.GetType(map.TargetType, false) ?? AppDomain.CurrentDomain.GetAssemblies().AsValueEnumerable()
                .Select(assembly => assembly.GetType(map.TargetType)).FirstOrDefault(type => type != null);

            if (clr == null)
            {
                continue;
            }

            map.TargetEntityType = clr;
            cache[entityName] = map;
        }

        return cache;
    }
}