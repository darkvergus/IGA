using System.Collections;
using System.Reflection;
using Core.Domain.Interfaces;
using Database.Context;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Database.Extensions;

public static class IgaDbContextBulkExtensions
{
    public static async Task BulkUpsertAsync<T>(this IgaDbContext context, IReadOnlyCollection<T>? entities, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (entities is null || entities.Count == 0)
        {
            return;
        }

        BulkConfig cfg = new()
        {
            PreserveInsertOrder = false,
            SetOutputIdentity = true,
            UpdateByProperties = ["Id"],
            PropertiesToExcludeOnUpdate = ["Version"]
        };

        await context.BulkInsertOrUpdateAsync(entities, cfg, cancellationToken: cancellationToken);
    }

    public static async Task BulkUpsertGenericAsync(DbContext db, Type entityType, IEnumerable<object> source, CancellationToken cancellationToken)
    {
        Type listType = typeof(List<>).MakeGenericType(entityType);
        IList list = (IList)Activator.CreateInstance(listType)!;

        foreach (object o in source)
        {
            list.Add(o);
        }

        MethodInfo method = typeof(IgaDbContextBulkExtensions).GetMethod(nameof(BulkUpsertAsync))!.MakeGenericMethod(entityType);

        await (Task)method.Invoke(null, [db, list, cancellationToken])!;
    }
}