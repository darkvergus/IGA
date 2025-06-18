using Core.Dynamic;
using CsvCollector.Repository;
using CsvCollector.Source;
using Database.Context;
using Domain.Mapping;
using Ingestion.Interfaces;
using Ingestion.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CsvCollector;

public sealed class CsvCollector(IServiceProvider root) : ICollector
{
    public string ConnectorName => "CsvCollector";

    private IConfiguration? configuration;
    private ILogger? logger;

    public void Initialize(IConfiguration cfg, ILogger log)
    {
        configuration = cfg;
        logger = log;
    }

    public async Task RunAsync(IReadOnlyDictionary<string, string> args, CancellationToken cancellationToken = default)
    {
        if (!args.TryGetValue("Path", out string? relativePath))
        {
            throw new ArgumentException("Path parameter missing");
        }

        string dllDir = Path.GetDirectoryName(typeof(CsvCollector).Assembly.Location)!;
        string fullPath = Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(dllDir, relativePath.Replace('/', Path.DirectorySeparatorChar));

        char delimiter = args.TryGetValue("Delimiter", out string? value) && !string.IsNullOrEmpty(value) ? value[0] : ',';

        string entity = args.TryGetValue("Entity", out string? ent) && !string.IsNullOrWhiteSpace(ent) ? ent : "identity";
        using IServiceScope scope = root.CreateScope();

        IgaDbContext context = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
        IngestionPipeline pipeline = new(context);
        CsvSource source = new(fullPath, delimiter);
        ImportMapping? importMapping = CsvMappingRepository.Get(entity);

        if (importMapping == null)
        {
            logger?.LogError($"No mapping found for entity '{entity}'. Job aborted.");

            return;
        }

        List<DynamicAttributeDefinition> attributeDefinitions = await context.DynamicAttributeDefinitions.ToListAsync(cancellationToken);
        
        try
        {
            await pipeline.RunAsync(source, importMapping, attributeDefinitions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"LDAP import failed for entity {entity}");
            return;
        }
       
        logger?.LogInformation($"CsvCollector finished importing {entity} rows from {Path.GetFileName(fullPath)}");

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}