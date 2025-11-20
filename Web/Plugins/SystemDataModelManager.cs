using System.Xml.Serialization;
using Core.Entities;
using Domain.Mapping;
using ZLinq;

namespace Web.Plugins;

public sealed class SystemDataModelManager(IWebHostEnvironment environment)
{
    public PluginDataModel? LoadDataModel(SystemConfiguration systemConfiguration, string entityName)
    {
        if (string.Equals(entityName, "identity", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(systemConfiguration.IdentityDataModelXml))
        {
            PluginDataModel? storedIdentityDataModel = DeserializeDataModel(systemConfiguration.IdentityDataModelXml);
            if (storedIdentityDataModel != null)
            {
                return storedIdentityDataModel;
            }
        }

        if (string.Equals(entityName, "permission", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(systemConfiguration.PermissionDataModelXml))
        {
            PluginDataModel? storedPermissionDataModel = DeserializeDataModel(systemConfiguration.PermissionDataModelXml);
            if (storedPermissionDataModel != null)
            {
                return storedPermissionDataModel;
            }
        }

        string pluginsRootPath = Path.Combine(environment.ContentRootPath, "Plugins");

        List<string> candidateDirectoryPaths = [];
        
        if (!string.IsNullOrWhiteSpace(systemConfiguration.CollectorName))
        {
            string collectorDirectoryPath = Path.Combine(pluginsRootPath, systemConfiguration.CollectorName);
            candidateDirectoryPaths.Add(collectorDirectoryPath);
        }

        PluginDataModel? pluginDataModelFromPlugins = candidateDirectoryPaths.AsValueEnumerable().Select(directoryPath => XmlDataModelLoader.Load(directoryPath, entityName)).OfType<PluginDataModel>().FirstOrDefault();

        return pluginDataModelFromPlugins;
    }

    private static PluginDataModel? DeserializeDataModel(string xml)
    {
        XmlSerializer serializer = new(typeof(PluginDataModel));
        using StringReader stringReader = new(xml);
        object? result = serializer.Deserialize(stringReader);
        PluginDataModel? pluginDataModel = result as PluginDataModel;
        return pluginDataModel;
    }
}