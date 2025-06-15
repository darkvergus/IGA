using System.Xml.Serialization;
using ZLinq;

namespace Ingestion.Mapping;

public class XmlMappingLoader
{
    public static ImportMapping? Load(string pluginBasePath, string entity)
    {
        string path = Path.Combine(pluginBasePath, "Mappings", $"{entity.ToLowerInvariant()}.mapping.xml");
        
        if (!File.Exists(path))
        {
            return null;
        }
        
        using FileStream fs = File.OpenRead(path);
        ImportMapping mapping = (ImportMapping)new XmlSerializer(typeof(ImportMapping)).Deserialize(fs)!;

        Type? clr = Type.GetType(mapping.TargetType, throwOnError: false) ?? AppDomain.CurrentDomain.GetAssemblies().AsValueEnumerable()
            .Select(assembly => assembly.GetType(mapping.TargetType)).FirstOrDefault(type => type != null);

        if (clr == null)
        {
            return null;
        }
        
        mapping.TargetEntityType = clr;
        return mapping;
    }
}