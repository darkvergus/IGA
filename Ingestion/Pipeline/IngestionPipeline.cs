using System.Linq.Dynamic.Core;
using System.Reflection;
using Core.Dynamic;
using Database.Context;
using Database.Extensions;
using Ingestion.Interfaces;
using Ingestion.Mapping;
using Ingestion.Services;
using Microsoft.EntityFrameworkCore;
using ZLinq;

namespace Ingestion.Pipeline;

public sealed class IngestionPipeline(IgaDbContext db)
{
    private const int BatchSize = 20000;

    public async Task RunAsync(IDataSource source, ImportMapping? mapping, IEnumerable<DynamicAttributeDefinition> definitions,
        CancellationToken cancellationToken = default)
    {
        bool originalDetect = db.ChangeTracker.AutoDetectChangesEnabled;
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            RowMapper mapper = new(definitions);

            if (mapping?.TargetEntityType != null)
            {
                MethodInfo setMethodInfo = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(mapping.TargetEntityType);
                IQueryable set = (IQueryable)setMethodInfo.Invoke(db, null)!;

                Dictionary<string, Skinny> skinny = await AsNoTrackingGeneric(mapping.TargetEntityType, set)
                    .Select("new(BusinessKey as Key, AttrHash, Id, Version)").Cast<dynamic>().ToDictionaryAsync(dynamic => (string)dynamic.Key,
                        dynamic => new Skinny((ulong)dynamic.AttrHash, (Guid)dynamic.Id, (int)dynamic.Version),
                        StringComparer.OrdinalIgnoreCase, cancellationToken);

                List<object> inserts = [];
                List<object> updates = [];
                DateTime utcNow = DateTime.UtcNow;

                foreach (IDictionary<string, string> row in source.ReadAsync(cancellationToken))
                {
                    object entity = mapper.MapRow(mapping.TargetEntityType, Guid.NewGuid(), row, mapping);

                    string keyProp = mapping.PrimaryKeyProperty ?? "BusinessKey";
                    PropertyInfo pk = mapping.TargetEntityType.GetProperty(keyProp)! ?? throw new InvalidOperationException($"Missing PK property {keyProp}");
                    string businessKey = (string)pk.GetValue(entity)!;
                    ulong hash = (ulong)mapping.TargetEntityType.GetProperty("AttrHash") !.GetValue(entity)!;

                    if (!skinny.TryGetValue(businessKey, out Skinny? value))
                    {
                        mapping.TargetEntityType.GetProperty("CreatedAt")!.SetValue(entity, utcNow);
                        inserts.Add(entity);
                    }
                    else if (value.AttrHash != hash)
                    {
                        mapping.TargetEntityType.GetProperty("Id") !.SetValue(entity, value.Id);
                        mapping.TargetEntityType.GetProperty("Version") !.SetValue(entity, value.Version + 1);
                        mapping.TargetEntityType.GetProperty("ModifiedAt")!.SetValue(entity, utcNow);
                        updates.Add(entity);
                    }

                    if (inserts.Count + updates.Count >= BatchSize)
                    {
                        await FlushAsync(mapping.TargetEntityType, inserts, updates, cancellationToken);
                    }
                }

                await FlushAsync(mapping.TargetEntityType, inserts, updates, cancellationToken);
            }
        } 
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
        }
    }

    private static IQueryable AsNoTrackingGeneric(Type type, IQueryable source)
    {
        MethodInfo methodInfo = typeof(EntityFrameworkQueryableExtensions).GetMethods().AsValueEnumerable().First(info =>
                info.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking) && info.GetParameters().Length == 1)
            .MakeGenericMethod(type);

        return (IQueryable)methodInfo.Invoke(null, [source])!;
    }

    private async Task FlushAsync(Type entityType, List<object> inserts, List<object> updates, CancellationToken ct)
    {
        if (inserts.Count > 0)
        {
            await db.BulkInsertGeneric(entityType, inserts, ct);
            inserts.Clear();
        }

        if (updates.Count > 0)
        {
            await db.BulkUpdateGeneric(entityType, updates, ct);
            updates.Clear();
        }

        db.ChangeTracker.Clear();
    }
}