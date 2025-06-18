using System.Xml.Serialization;
using Domain.Mapping;

namespace Web.Plugins;

public class PluginMappingManager(IWebHostEnvironment environment)
{
    private readonly string pluginsRoot = Path.Combine(environment.ContentRootPath, "Plugins");

    public IEnumerable<PluginMappingInfo> ListMappings()
    {
        foreach (string pluginDir in Directory.GetDirectories(pluginsRoot))
        {
            string mappingsDir = Path.Combine(pluginDir, "Mappings");
            if (!Directory.Exists(mappingsDir))
            {
                continue;
            }

            foreach (string filePath in Directory.EnumerateFiles(mappingsDir, "*.mapping.xml"))
            {
                ImportMapping? mapping = XmlMappingLoader.Load(pluginDir, Path.GetFileNameWithoutExtension(filePath));
                if (mapping != null)
                {
                    yield return new()
                    {
                        PluginName = Path.GetFileName(pluginDir),
                        EntityName = Path.GetFileNameWithoutExtension(filePath),
                        MappingData = mapping
                    };
                }
            }
        }
    }

    public void SaveMapping(string pluginName, string entity, ImportMapping mapping)
    {
        string pluginDir = Path.Combine(pluginsRoot, pluginName);
        string targetFile = Path.Combine(pluginDir, "Mappings", $"{entity.ToLowerInvariant()}.mapping.xml");

        using FileStream fs = File.Create(targetFile);
        new XmlSerializer(typeof(ImportMapping)).Serialize(fs, mapping);
    }
}