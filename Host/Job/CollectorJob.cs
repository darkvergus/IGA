namespace Host.Job;

public record CollectorJob(string ConnectorName, IReadOnlyDictionary<string,string> Parameters); 