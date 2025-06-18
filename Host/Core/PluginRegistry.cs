using Ingestion.Interfaces;
using Provisioning.Interfaces;

namespace Host.Core;

public sealed class PluginRegistry
{
    private readonly Dictionary<int, ICollector> collectors = new();
    private readonly Dictionary<int, IProvisioner> provisioners = new();

    public void AddCollector(int instanceId, ICollector collector) => collectors[instanceId] = collector;
    public void AddProvisioner(int instanceId, IProvisioner provisioner) => provisioners[instanceId] = provisioner;

    public ICollector GetCollector(int instanceId) => collectors[instanceId];
    public IProvisioner GetProvisioner(int instanceId) => provisioners[instanceId];
}