using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ingestion.Interfaces;

public interface ICollector
{
    string ConnectorName { get; }
    void Initialize(IConfiguration cfg, ILogger log);
    Task RunAsync(IReadOnlyDictionary<string,string> dictionary, CancellationToken cancellationToken);
}