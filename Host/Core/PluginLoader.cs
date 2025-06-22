using System.Reflection;
using System.Text;
using Database.Context;
using Domain.Core.Entities.Provision;
using Domain.Mapping;
using Domain.Repository;
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
                try
                {
                    PluginLoadContext context = new(dll);
                    assemblyPath = context.LoadFromAssemblyPath(Path.GetFullPath(dll));
                    cache[dll] = assemblyPath;

                    foreach (KeyValuePair<string, ImportMapping> kv in MappingLoader.Load(assemblyPath))
                    {
                        MappingRepository.Register(kv.Key, kv.Value, assemblyPath.GetName().Name!);
                    }
                }
                catch (Exception ex)
                {
                    throw new($"Failed to load {dll}.", ex);
                }
                
            }

            IEnumerable<Type> types;
            try
            {
                types = assemblyPath.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null)!;
            }
            
            foreach (Type type in types.Where(type => contract.IsAssignableFrom(type) && !type.IsAbstract))
            {
                using IServiceScope scope = services.CreateScope();
                IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                if (typeLabel == "Collector")
                {
                    string? raw = db.ConnectorConfigs.AsValueEnumerable()
                        .Where(connector => connector.IsEnabled && connector.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase) && connector.Type == typeLabel)
                        .Select(connector => connector.ConfigData).SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    IConfiguration cfg = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(raw))).Build();

                    ICollector collector = (ICollector)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
                    collector.Initialize(cfg, logFactory.CreateLogger(type));
                    registry.AddCollector(collector.Name, collector);

                    yield return (T)collector;
                }
                else if (typeLabel == "Provisioner")
                {
                    string? raw = db.ProvisionConfigs.AsValueEnumerable()
                        .Where(provisioner => provisioner.IsEnabled && provisioner.Name.Equals(type.Name, StringComparison.OrdinalIgnoreCase) && provisioner.Type == typeLabel)
                        .Select(provisioner => provisioner.ConfigData).SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    IConfiguration cfg = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(raw))).Build();

                    IProvisioner provisioner = (IProvisioner)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);

                    provisioner.Initialize(cfg, logFactory.CreateLogger(type));
                    registry.AddProvisioner(provisioner.Name, provisioner);

                    yield return (T)provisioner;
                }
            }
        }
    }
}