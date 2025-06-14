using Host.Interfaces;
using Host.Job;
using Ingestion.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Provisioning.Interfaces;

namespace Host.Core;

public sealed class Worker(IConnectorQueue queue, Dictionary<string, ICollector> collectors, Dictionary<string, IProvisioner> provisioners, ILogger<Worker> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            while (queue.TryDequeueCollector(out CollectorJob? collectorJob))
            {
                if (!collectors.TryGetValue(collectorJob!.ConnectorName, out ICollector? plugin))
                {
                    log.LogWarning($"Collector not found: {collectorJob.ConnectorName}");

                    continue;
                }

                await plugin.RunAsync(collectorJob.Parameters, cancellationToken);
            }

            while (queue.TryDequeueProvisioner(out ProvisionerJob? provisionerJob))
            {
                if (!provisioners.TryGetValue(provisionerJob!.ConnectorName, out IProvisioner? plugin))
                {
                    log.LogWarning($"Provisioner not found: {provisionerJob.ConnectorName}");

                    continue;
                }

                await plugin.ProvisionAsync(provisionerJob.Payload, cancellationToken);
            }

            await Task.Delay(50, cancellationToken);
        }
    }
}