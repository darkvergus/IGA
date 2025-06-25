using System.Text.Json;
using Core.Common;
using Database.Context;
using Domain.Core.Entities;
using Domain.Jobs;
using Host.Core;
using Ingestion.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Provisioning;
using Provisioning.Interfaces;

namespace Host.Workers;

public sealed class PipelineWorker(IServiceProvider root, ILogger<PipelineWorker> log, PluginRegistry registry, InMemJobQueue queue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (JobEnvelope envelope in queue.ReadAllAsync(cancellationToken))
        {
            using IServiceScope scope = root.CreateScope();
            IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
            Job? job = await db.Jobs.FindAsync([envelope.Id], cancellationToken);

            if (job is null)
            {
                continue;
            }

            job.Status = JobStatus.InProgress;
            job.StartedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            try
            {
                switch (envelope.Type)
                {
                    case JobType.Ingestion:
                        Dictionary<string, string>? args = JsonSerializer.Deserialize<Dictionary<string, string>>(envelope.PayloadJson)!;
                        ICollector collector = registry.GetCollector(job.Name);
                        await collector.RunAsync(args, cancellationToken);

                        break;
                    case JobType.Provisioning:
                        ProvisioningCommand command = JsonSerializer.Deserialize<ProvisioningCommand>(job.PayloadJson)!;
                        IProvisioner provisioner = registry.GetProvisioner(job.Name);
                        ProvisionResult result = await provisioner.RunAsync(command, cancellationToken);

                        if (!result.Success)
                        {
                            job.Status = JobStatus.Failed;
                            job.Error = result.Details ?? "Provisioner returned failure.";
                        }

                        break;
                }

                if (job.Status != JobStatus.Failed)
                {
                    job.Status = JobStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                job.Error = ex.Message;
            }
            finally
            {
                job.FinishedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}