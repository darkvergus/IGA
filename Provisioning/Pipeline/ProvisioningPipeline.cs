using System.Reflection;
using Core.Dynamic;
using Database.Context;
using Domain.Mapping;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Provisioning.Enums;
using Provisioning.Interfaces;
using ZLinq;

namespace Provisioning.Pipeline;

public sealed class ProvisioningPipeline(IgaDbContext db, ILogger<ProvisioningPipeline> log)
{
    private const int BatchSize = 20_000;

    public async Task RunAsync(ImportMapping mapping, IEnumerable<DynamicAttributeDefinition> definitions, IProvisioner provisioner,
        ProvisioningOperation operation,
        Func<IQueryable, IQueryable>? filter = null, CancellationToken cancellationToken = default)
    {
        if (mapping.TargetEntityType == null)
        {
            throw new InvalidOperationException("ImportMapping is missing TargetEntityType.");
        }

        MethodInfo setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(mapping.TargetEntityType);

        IQueryable set = (IQueryable)setMethod.Invoke(db, null)!;

        if (filter != null)
        {
            set = filter(set);
        }

        List<object> batch = [];
        RowMapper mapper = new(definitions);

        await foreach (object entity in AsAsyncEnumerableGeneric(mapping.TargetEntityType, set).WithCancellation(cancellationToken))
        {
            batch.Add(entity);

            if (batch.Count >= BatchSize)
            {
                await FlushAsync(batch, mapping, mapper, provisioner, operation, cancellationToken);
            }
        }

        await FlushAsync(batch, mapping, mapper, provisioner, operation, cancellationToken);
    }

    private static async Task FlushAsync(List<object> entities, ImportMapping mapping, RowMapper mapper, IProvisioner provisioner,
        ProvisioningOperation operation, CancellationToken cancellationToken)
    {
        object[] batch = entities.ToArray();
        entities.Clear();

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = 8,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(batch, options, async (entity, token) =>
        {
            Dictionary<string, string> outbound = BuildOutboundBag(entity, mapping, mapper);
            string externalId = ResolveExternalId(entity, mapping);
            ProvisioningCommand command = new(operation, externalId, outbound);
            await provisioner.RunAsync(command, token);
        });
    }

    private static Dictionary<string, string> BuildOutboundBag(object entity, ImportMapping mapping, RowMapper mapper)
    {
        Dictionary<string, string> row = new(StringComparer.OrdinalIgnoreCase);

        foreach (ImportMappingItem mappingItem in mapping.FieldMappings)
        {
            string propName = mappingItem.SourceFieldName;
            PropertyInfo? propertyInfo = entity.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo?.GetValue(entity) is { } val)
            {
                row[propName] = val.ToString()!;
            }
        }

        mapper.MapRow(mapping.TargetEntityType!, Guid.NewGuid(), row, null);

        Dictionary<string, string> outboundBag = new(StringComparer.OrdinalIgnoreCase);

        foreach (ImportMappingItem mappingItem in mapping.FieldMappings)
        {
            if (row.TryGetValue(mappingItem.SourceFieldName, out string? v))
            {
                outboundBag[mappingItem.TargetFieldName] = v;
            }
        }

        return outboundBag;
    }

    private static string ResolveExternalId(object entity, ImportMapping map)
    {
        string keyProp = map.PrimaryKeyProperty ?? "BusinessKey";
        PropertyInfo? propertyInfo = entity.GetType().GetProperty(keyProp, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo != null && propertyInfo.GetValue(entity) is string key && !string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        PropertyInfo idProp = entity.GetType().GetProperty("Id")!;

        return idProp.GetValue(entity)!.ToString()!;
    }

    private static IAsyncEnumerable<object> AsAsyncEnumerableGeneric(Type type, IQueryable source)
    {
        MethodInfo methodInfo = typeof(EntityFrameworkQueryableExtensions).GetMethods().AsValueEnumerable().First(methodInfo =>
                methodInfo is { Name: nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable), IsGenericMethodDefinition: true })
            .MakeGenericMethod(type);

        object enumerable = methodInfo.Invoke(null, [source])!;

        return (IAsyncEnumerable<object>)enumerable;
    }
}