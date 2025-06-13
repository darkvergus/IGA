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
    private const int BatchSize = 10000;

    public async Task RunAsync(IDataSource source, ImportMapping mapping, IEnumerable<DynamicAttributeDefinition> definitions,
        CancellationToken cancellationToken = default)
    {
        RowMapper rowMapper = new(definitions);
        List<object> batch = new(BatchSize);

        Dictionary<string, string> requiredFieldMap = definitions
            .AsValueEnumerable().Where(attributeDefinition => attributeDefinition.IsRequired)
            .ToDictionary(
                attributeDefinition => attributeDefinition.SystemName,
                attributeDefinition => mapping.FieldMappings.AsValueEnumerable()
                    .First(mappingItem => mappingItem.TargetFieldName == attributeDefinition.SystemName).SourceFieldName,
                StringComparer.OrdinalIgnoreCase);

        foreach (IDictionary<string, string> row in source.ReadRecords())
        {
            bool missingRequired = requiredFieldMap.Any(kvp => !row.TryGetValue(kvp.Value, out string? value) || string.IsNullOrWhiteSpace(value));

            if (missingRequired)
            {
                continue;
            }

            Guid entityId = Guid.NewGuid();
            object obj = rowMapper.MapRow(mapping.TargetEntityType, entityId, row, mapping);
            batch.Add(obj);

            if (batch.Count >= BatchSize)
            {
                await PersistBatchAsync(mapping.TargetEntityType, batch, cancellationToken);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await PersistBatchAsync(mapping.TargetEntityType, batch, cancellationToken);
        }
    }

    private Task PersistBatchAsync(Type entityType, List<object> objects, CancellationToken cancellationToken) => IgaDbContextBulkExtensions.BulkUpsertGenericAsync(db, entityType, objects, cancellationToken);
}