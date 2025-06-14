using Host.Job;

namespace Host.Interfaces;

public interface IConnectorQueue
{
    void Enqueue(CollectorJob  job);
    void Enqueue(ProvisionerJob job);
    bool TryDequeueCollector  (out CollectorJob?  job);
    bool TryDequeueProvisioner(out ProvisionerJob? job);
}