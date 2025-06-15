using System.Xml.Serialization;

namespace Ingestion.Mapping;

public class XmlDataModelLoader
{
    public static PluginDataModel? Load(string pluginBasePath, string entity)
    {
        string path = Path.Combine(pluginBasePath, "Models", $"{entity.ToLowerInvariant()}.datamodel.xml");
        
        if (!File.Exists(path))
        {
            return null;
        }
        
        using FileStream stream = File.OpenRead(path);
        return (PluginDataModel)new XmlSerializer(typeof(PluginDataModel)).Deserialize(stream)!;
    }
}