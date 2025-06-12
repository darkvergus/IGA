using Core.Domain.Dynamic;
using Database.Context;
using Ingestion.Mapping;
using Ingestion.Pipeline;
using Ingestion.Repository;
using Ingestion.Source;
using Microsoft.EntityFrameworkCore;

namespace Web.Extensions;

public class IngestionExtensions
{
    public static async Task RunCsvAsync(string entity, string filePath, IngestionPipeline pipeline, IgaDbContext db, CancellationToken cancellationToken)
    {
        List<DynamicAttributeDefinition> defs = await db.DynamicAttributeDefinitions.ToListAsync(cancellationToken);
        ImportMapping map = CsvMappingRepository.Get(entity);
        CsvSource src = new(filePath);

        await pipeline.RunAsync(src, map, defs, cancellationToken);
    }
}