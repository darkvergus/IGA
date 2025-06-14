using Core.Common;

namespace Host.Job;

public record ProvisionerJob(string ConnectorName, Entity<object> Payload);