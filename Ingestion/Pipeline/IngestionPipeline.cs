using Core.Domain.Dynamic;
using Database.Context;
using Database.Extensions;
using Ingestion.Interfaces;
using Ingestion.Mapping;
using Ingestion.Services;
using ZLinq;

namespace Ingestion.Pipeline;

public sealed class IngestionPipeline(IgaDbContext db)
{
    public async Task RunAsync(IDataSource source, ImportMapping mapping, IEnumerable<DynamicAttributeDefinition> definitions,
        CancellationToken cancellationToken = default)
    {
        RowMapper rowMapper = new(definitions);
        List<object> results = [];
        HashSet<string> seen = [];

        foreach (IDictionary<string, string> row in source.ReadRecords())
        {
            if (definitions.AsValueEnumerable().Any(attributeDefinition => attributeDefinition.IsRequired && mapping.FieldMappings.AsValueEnumerable().Any(importMappingItem =>
                                                           importMappingItem.TargetFieldName == attributeDefinition.SystemName) &&
                                                       (!row.TryGetValue(mapping.FieldMappings.AsValueEnumerable().First(importMappingItem =>
                                                                importMappingItem.TargetFieldName == attributeDefinition.SystemName).SourceFieldName,
                                                            out string? value) ||
                                                        string.IsNullOrWhiteSpace(value))))
            {
                continue;
            }

            Guid entityId = Guid.NewGuid();
            object obj = rowMapper.MapRow(mapping.TargetEntityType, entityId, row, mapping);

            results.Add(obj);
        }

        if (results.Count > 0)
        {
            await IgaDbContextBulkExtensions.BulkUpsertGenericAsync(db, mapping.TargetEntityType, results, cancellationToken);
        }
    }
}