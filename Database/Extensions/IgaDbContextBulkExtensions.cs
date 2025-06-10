using Core.Domain.Entities;
using Database.Context;
using EFCore.BulkExtensions;

namespace Database.Extensions;

public static class IgaDbContextBulkExtensions
{
    public static async Task BulkUpsertAsync<T>(this IgaDbContext ctx, IReadOnlyCollection<T>? entities, CancellationToken ct = default) where T : Entity<object>
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

        await ctx.BulkInsertOrUpdateAsync(entities, cfg, cancellationToken: ct);
    }
}
