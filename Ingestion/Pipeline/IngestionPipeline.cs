using System.Linq.Dynamic.Core;
using System.Reflection;
using Core.Dynamic;
using Database.Context;
using Database.Extensions;
using Domain.Interfaces;
using Domain.Mapping;
using Domain.Services;
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

                List<(object entity, bool isInsert)> ops = [];

                foreach (IDictionary<string, string> row in source.ReadAsync(cancellationToken))
                {
                    object entity = mapper.MapRow(mapping.TargetEntityType, Guid.NewGuid(), row, mapping);
                    DateTime now = DateTime.UtcNow;
                    
                    string keyProp = mapping.PrimaryKeyProperty ?? "BusinessKey";
                    PropertyInfo propertyInfo = mapping.TargetEntityType.GetProperty(keyProp)! ?? throw new InvalidOperationException($"Missing PK property {keyProp}");
                    string businessKey = (string)propertyInfo.GetValue(entity)!;
                    ulong hash = (ulong)mapping.TargetEntityType.GetProperty("AttrHash") !.GetValue(entity)!;

                    if (!skinny.TryGetValue(businessKey, out Skinny? value))
                    {
                        mapping.TargetEntityType.GetProperty("CreatedAt")!.SetValue(entity, now);
                        ops.Add((entity, true)); 
                    }
                    else if (value.AttrHash != hash)
                    {
                        mapping.TargetEntityType.GetProperty("Id") !.SetValue(entity, value.Id);
                        mapping.TargetEntityType.GetProperty("Version") !.SetValue(entity, value.Version + 1);
                        mapping.TargetEntityType.GetProperty("ModifiedAt")!.SetValue(entity, now);
                        ops.Add((entity, false));
                    }

                    if (ops.Count >= BatchSize)
                    {
                        await FlushAsync(mapping.TargetEntityType, ops, cancellationToken);
                    }
                }

                if (ops.Count > 0)
                {
                    await FlushAsync(mapping.TargetEntityType, ops, cancellationToken);
                }
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

    private async Task FlushAsync(Type entityType, List<(object entity, bool isInsert)> ops, CancellationToken ct)
    {
        if (ops.Count == 0)
        {
            return;
        }

        bool currentIsInsert = ops[0].isInsert;
        List<object> buffer = [];

        foreach ((object entity, bool isInsert) in ops)
        {
            if (isInsert == currentIsInsert)
            {
                buffer.Add(entity);
            }
            else
            {
                if (currentIsInsert)
                {
                    await db.BulkInsertGeneric(entityType, buffer, ct);
                }
                else
                {
                    await db.BulkUpdateGeneric(entityType, buffer, ct);
                }
                
                buffer.Clear();
                buffer.Add(entity);
                currentIsInsert = isInsert;
            }
        }
        
        if (buffer.Count > 0)
        {
            if (currentIsInsert)
            {
                await db.BulkInsertGeneric(entityType, buffer, ct);
            }
            else
            {
                await db.BulkUpdateGeneric(entityType, buffer, ct);
            }
        }

        ops.Clear();
        db.ChangeTracker.Clear();
    }
}