using Ingestion.Interfaces;
using Provisioning.Interfaces;

namespace Host.Core;

public sealed class PluginRegistry
{
    private readonly Dictionary<string, ICollector> collectors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IProvisioner> provisioners = new(StringComparer.OrdinalIgnoreCase);

    public void AddCollector(string name, ICollector collector) => collectors[name] = collector;
    public void AddProvisioner(string name, IProvisioner provisioner) => provisioners[name] = provisioner;

    public ICollector GetCollector(string name) => collectors[name];
    public IProvisioner GetProvisioner(string name) => provisioners[name];
    
    public IEnumerable<IProvisioner> GetAllProvisioners() => provisioners.Values;
    public IEnumerable<ICollector> GetAllCollectors() => collectors.Values;
}