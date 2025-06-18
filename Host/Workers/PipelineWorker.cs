using System.Text.Json;
using Core.Common;
using Database.Context;
using Domain.Core.Entities;
using Domain.Jobs;
using Host.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Host.Workers;

public sealed class PipelineWorker(IServiceProvider root, ILogger<PipelineWorker> log, PluginRegistry registry, InMemJobQueue queue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stopping)
    {
        await foreach (JobEnvelope envelope in queue.ReadAllAsync(stopping))
        {
            using IServiceScope scope = root.CreateScope();
            IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
            Job? job = await db.Jobs.FindAsync([envelope.Id], stopping);

            if (job is null)
            {
                continue;
            }

            job.Status = JobStatus.InProgress;
            job.StartedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(stopping);

            try
            {
                switch (envelope.Type)
                {
                    case JobType.Ingestion:
                        Dictionary<string, string>? args = JsonSerializer.Deserialize<Dictionary<string, string>>(envelope.PayloadJson)!;
                        await registry.GetCollector(envelope.ConnectorInstanceId).RunAsync(args, stopping);

                        break;
                    case JobType.Provisioning:
                        Entity<object>? entity = JsonSerializer.Deserialize<Entity<object>>(envelope.PayloadJson)!;
                        await registry.GetProvisioner(envelope.ConnectorInstanceId).ProvisionAsync(entity, stopping);

                        break;
                }

                job.Status = JobStatus.Completed;
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                job.Error = ex.Message;
            }

            job.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(stopping);
        }
    }
}