using Database.Context;
using Domain.Core.Entities;
using Domain.Jobs;
using Host.Core;
using Microsoft.EntityFrameworkCore;

namespace Host.Services;

public sealed class JobService(IgaDbContext db, InMemJobQueue queue)
{
    public async Task<long> EnqueueAsync(JobType type, string connectorName, int instanceId, string payload, CancellationToken cancellationToken = default)
    {
        Job job = new()
        {
            Type = type,
            Name = connectorName,
            InstanceId = instanceId,
            PayloadJson = payload
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        await queue.EnqueueAsync(new(job.Id, type, instanceId, payload), cancellationToken);

        return job.Id;
    }

    public Task<Job?> GetAsync(long id, CancellationToken cancellationToken = default) => db.Jobs.AsNoTracking().SingleOrDefaultAsync(job => job.Id == id, cancellationToken);
}