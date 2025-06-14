using System.Threading.Channels;
using Host.Job;

namespace Host.Interfaces;

public sealed class InMemConnectorQueue : IConnectorQueue
{
    private readonly Channel<CollectorJob> collectorJob = Channel.CreateUnbounded<CollectorJob>();
    private readonly Channel<ProvisionerJob> provisioningJob = Channel.CreateUnbounded<ProvisionerJob>();

    public void Enqueue(CollectorJob job) => collectorJob.Writer.TryWrite(job);
    public void Enqueue(ProvisionerJob job) => provisioningJob.Writer.TryWrite(job);
    public bool TryDequeueCollector(out CollectorJob? job) => collectorJob.Reader.TryRead(out job);
    public bool TryDequeueProvisioner(out ProvisionerJob? job) => provisioningJob.Reader.TryRead(out job);
}