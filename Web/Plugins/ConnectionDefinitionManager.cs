using Domain.Connection;

namespace Web.Plugins;

public sealed class ConnectionDefinitionManager(IWebHostEnvironment webHostEnvironment)
{
    private readonly string pluginsRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "Plugins");

    public ConnectionDefinition? LoadCollectorDefinition(string collectorName)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            return null;
        }

        string pluginDirectoryPath = Path.Combine(pluginsRootPath, collectorName);
        ConnectionDefinition? connectionDefinition = XmlConnectionDefinitionLoader.Load(pluginDirectoryPath);
        return connectionDefinition;
    }

    public ConnectionDefinition? LoadProvisionerDefinition(string provisionerName)
    {
        if (string.IsNullOrWhiteSpace(provisionerName))
        {
            return null;
        }

        string pluginDirectoryPath = Path.Combine(pluginsRootPath, provisionerName);
        ConnectionDefinition? connectionDefinition = XmlConnectionDefinitionLoader.Load(pluginDirectoryPath);
        return connectionDefinition;
    }
}