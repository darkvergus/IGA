namespace Domain.Jobs;

public readonly record struct JobEnvelope(long Id, JobType Type, int ConnectorInstanceId, string PayloadJson);