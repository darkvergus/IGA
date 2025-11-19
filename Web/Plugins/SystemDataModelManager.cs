using Core.Entities;
using Domain.Mapping;
using ZLinq;

namespace Web.Plugins;

public sealed class SystemDataModelManager(IWebHostEnvironment environment)
{
    public PluginDataModel? LoadDataModel(SystemConfiguration systemConfiguration, string entityName)
    {
        string pluginsRootPath = Path.Combine(environment.ContentRootPath, "Plugins");

        List<string> candidateDirectoryPaths = [];

        if (!string.IsNullOrWhiteSpace(systemConfiguration.Type))
        {
            string typeDirectoryPath = Path.Combine(pluginsRootPath, systemConfiguration.Type);
            candidateDirectoryPaths.Add(typeDirectoryPath);
        }

        if (!string.IsNullOrWhiteSpace(systemConfiguration.CollectorName))
        {
            string collectorDirectoryPath = Path.Combine(pluginsRootPath, systemConfiguration.CollectorName);
            candidateDirectoryPaths.Add(collectorDirectoryPath);
        }

        return candidateDirectoryPaths.AsValueEnumerable().Select(candidateDirectoryPath => XmlDataModelLoader.Load(candidateDirectoryPath, entityName)).OfType<PluginDataModel>().FirstOrDefault();
    }
}