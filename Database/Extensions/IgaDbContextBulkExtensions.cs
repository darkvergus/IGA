using System.Collections;
using System.Reflection;
using Core.Domain.Interfaces;
using Database.Context;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ZLinq;

namespace Database.Extensions;

public static class IgaDbContextBulkExtensions
{
    public static async Task BulkInsertGeneric(this IgaDbContext context, Type entityType, IList list, CancellationToken cancellationToken,
        Action<BulkConfig>? cfgModifier = null)
    {
        if (list.Count == 0)
        {
            return;
        }

        BulkConfig cfg = new()
        {
            SetOutputIdentity = true
        };
        cfgModifier?.Invoke(cfg);

        await context.BulkInsertAsync(list.Cast<object>(), cfg, null, entityType, cancellationToken: cancellationToken);
    }

    public static async Task BulkUpdateGeneric(this IgaDbContext context, Type entityType, IList list, CancellationToken cancellationToken,
        Action<BulkConfig>? cfgModifier = null)
    {
        if (list.Count == 0)
        {
            return;
        }

        BulkConfig cfg = new()
        {
            UpdateByProperties = ["BusinessKey"]
        };
        cfgModifier?.Invoke(cfg);

        await context.BulkUpdateAsync(list.Cast<object>(), cfg, null, entityType, cancellationToken: cancellationToken);
    }
}