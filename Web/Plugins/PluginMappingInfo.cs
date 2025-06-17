using Ingestion.Mapping;

namespace Web.Plugins;

public class PluginMappingInfo
{
    public string PluginName { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public ImportMapping MappingData { get; set; } = null!;
}