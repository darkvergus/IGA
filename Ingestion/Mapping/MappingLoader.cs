using System.Reflection;
using System.Text.Json;
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
        
        foreach (string file in Directory.EnumerateFiles(path, "*.json"))
        {
            Console.WriteLine($"[Mapper] inspect {file}");

            try
            {
                JsonDocument document = JsonDocument.Parse(File.ReadAllText(file));

                if (!document.RootElement.TryGetProperty("targetType", out JsonElement typeEl))
                {
                    continue;
                }

                Type? clr = Type.GetType(typeEl.GetString()!, false) ?? AppDomain.CurrentDomain.GetAssemblies().AsValueEnumerable()
                    .Select(assembly => assembly.GetType(typeEl.GetString()!)).FirstOrDefault(type => type != null);

                if (clr is null)
                {
                    continue;
                }

                ImportMapping mapping = new(clr);

                foreach (JsonElement element in document.RootElement.GetProperty("fields").EnumerateArray())
                {
                    mapping.FieldMappings.Add(new ImportMappingItem(element.GetProperty("source").GetString()!,
                        element.GetProperty("target").GetString()!));
                }

                string key = Path.GetFileNameWithoutExtension(file);
                cache[key] = mapping;
            }
            catch
            {
                /* bad file – ignore */
            }
        }

        return cache;
    }
}