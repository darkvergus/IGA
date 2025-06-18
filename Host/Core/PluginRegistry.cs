using Ingestion.Interfaces;
using Provisioning.Interfaces;

namespace Host.Core;

public sealed class PluginRegistry
{
    private readonly Dictionary<string, ICollector> collectors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, IProvisioner> provisioners = new();

    public void AddCollector(string name, ICollector collector) => collectors[name] = collector;
    public void AddProvisioner(int instanceId, IProvisioner provisioner) => provisioners[instanceId] = provisioner;

    public ICollector GetCollector(string name) => collectors[name];
    public IProvisioner GetProvisioner(int instanceId) => provisioners[instanceId];
}