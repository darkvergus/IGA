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

public sealed class PluginLoader(string pluginRoot, IServiceProvider services, ILoggerFactory logFactory, PluginRegistry registry)
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
                PluginLoadContext ctx = new(dll);
                assemblyPath = ctx.LoadFromAssemblyPath(Path.GetFullPath(dll));
                cache[dll] = assemblyPath;
            }

            foreach (Type type in assemblyPath.GetTypes().Where(type => contract.IsAssignableFrom(type) && !type.IsAbstract))
            {
                using IServiceScope scope = services.CreateScope();
                IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                if (typeLabel == "Collector")
                {
                    string? raw = db.ConnectorConfigs.AsValueEnumerable()
                        .Where(connector => connector.IsEnabled && connector.Name == type.Name && connector.Type == typeLabel)
                        .Select(connector => connector.ConfigData).SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    IConfiguration cfg = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(raw))).Build();

                    ICollector collector = (ICollector)ActivatorUtilities.CreateInstance(services, type);
                    collector.Initialize(cfg, logFactory.CreateLogger(type));
                    registry.AddCollector(collector.ConnectorName, collector);

                    yield return (T) collector;
                }
            }
        }
    }
}