using Core.Dynamic;
using CsvCollector.Repository;
using CsvCollector.Source;
using Database.Context;
using Ingestion.Interfaces;
using Ingestion.Mapping;
using Ingestion.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CsvCollector;

public sealed class CsvCollector(IServiceProvider root, ILogger<CsvCollector> log) : ICollector
{
    public string ConnectorName => "CsvCollector";
    private readonly string dir = Path.GetDirectoryName(typeof(CsvCollector).Assembly.Location)!;
    
    private IConfiguration? cfg;
    private ILogger? log;
    
    public void Initialize(IConfiguration cfg, ILogger log)
    {
        this.cfg = cfg;
        this.log = log;
    }
    
    public async Task RunAsync(IReadOnlyDictionary<string, string> dictionary, CancellationToken cancellationToken)
    {
        if (!dictionary.TryGetValue("Path", out string? rel))
        {
            log?.LogWarning("CsvCollector missing Path param");

            return;
        }

        if (!dictionary.TryGetValue("Entity", out string? entity))
        {
            entity = "identity";
        }

        char delimiter = dictionary.TryGetValue("Delimiter", out string? value) && !string.IsNullOrEmpty(value) ? value[0] : ',';

        string full = Path.Combine(dir, rel.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(full))
        {
            log?.LogWarning($"File not found {full}");

            return;
        }

        using IServiceScope scope = root.CreateScope();
        IgaDbContext context = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
        IngestionPipeline pipeline = new(context);
        CsvSource source = new(full, delimiter);
        ImportMapping importMapping = CsvMappingRepository.Get(entity);
        List<DynamicAttributeDefinition> attributeDefinitions = await context.DynamicAttributeDefinitions.ToListAsync(cancellationToken);

        await pipeline.RunAsync(source, importMapping, attributeDefinitions, cancellationToken);
        File.Delete(full);
    }
}