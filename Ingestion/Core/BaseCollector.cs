using Ingestion.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ingestion.Core;

/// <summary>Reusable common behaviour (logging, stopwatch, etc.)</summary>
public abstract class BaseCollector : ICollector
{
    protected IConfiguration Config { get; private set; } = default!;
    protected ILogger Logger { get; private set; } = default!;

    public abstract string Name { get; }

    public virtual void Initialize(IConfiguration cfg, ILogger log) => (Config, Logger) = (cfg, log);
    public abstract Task RunAsync(IReadOnlyDictionary<string,string> parameters, CancellationToken cancellationToken);
}