using Domain.Jobs;

namespace Domain.Core.Entities;

public sealed class Job
{
    public long Id { get; set; }
    public JobType Type { get; set; }
    public string ConnectorName { get; set; } = null!;
    public int ConnectorInstanceId { get; set; }
    public string PayloadJson { get; set; } = null!;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? Error { get; set; }
}
