using System.Reflection;
using System.Text;
using Database.Context;
using Ingestion.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Provisioning.Interfaces;
using ZLinq;

namespace Host.Core;

public sealed class PluginLoader(string pluginRoot, IServiceProvider services, ILoggerFactory logFactory)
{
    public IEnumerable<ICollector> LoadCollectors() => Load<ICollector>("Collector");
    public IEnumerable<IProvisioner> LoadProvisioners() => Load<IProvisioner>("Provisioner");
    
    private readonly Dictionary<string, Assembly> cache = new();

    private IEnumerable<T> Load<T>(string typeLabel)
    {
        Type contract = typeof(T);

        foreach (string dll in Directory.EnumerateFiles(pluginRoot, "*.dll"))
        {
            if (!cache.TryGetValue(dll, out Assembly? assemblyPath))
            {
                PluginLoadContext context = new(dll);
                assemblyPath = context.LoadFromAssemblyPath(Path.GetFullPath(dll));
                cache[dll] = assemblyPath;
            }

            foreach (Type type in assemblyPath.GetTypes().Where(type => contract.IsAssignableFrom(type) && !type.IsAbstract))
            {
                T instance = (T)ActivatorUtilities.CreateInstance(services, type);

                using IServiceScope scope = services.CreateScope();
                IgaDbContext dbContext = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                string? raw = dbContext.ConnectorConfigs.AsValueEnumerable()
                    .Where(config => config.IsEnabled && config.ConnectorName == type.Name && config.ConnectorType == typeLabel)
                    .Select(config => config.ConfigData).SingleOrDefault();

                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                IConfigurationRoot cfg = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(raw))).Build();

                switch (instance)
                {
                    case ICollector collector: 
                        collector.Initialize(cfg, logFactory.CreateLogger(type)); 
                        break;
                    case IProvisioner provisioner: 
                        provisioner.Initialize(cfg, logFactory.CreateLogger(type)); 
                        break;
                }

                yield return instance;
            }
        }
    }
}