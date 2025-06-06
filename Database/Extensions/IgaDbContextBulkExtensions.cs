using Core.Domain.Records;
using Database.Context;
using EFCore.BulkExtensions;

namespace Database.Extensions;

public static class IgaDbContextBulkExtensions
{
    /// <summary>
    /// Upserts <see cref="Identity"/> entities with a single round‑trip using the
    /// EFCore.BulkExtensions library.
    /// </summary>
    public static async Task BulkUpsertIdentitiesAsync(this IgaDbContext ctx, IReadOnlyCollection<Identity>? identities, CancellationToken ct = default)
    {
        if (identities is null || identities.Count == 0)
        {
            return;
        }

        BulkConfig bulkConfig = new()
        {
            PreserveInsertOrder = false,
            SetOutputIdentity = true,
            UpdateByProperties = [nameof(Identity.Id)],
            PropertiesToExcludeOnUpdate = [nameof(Identity.Version)]
        };

        await ctx.BulkInsertOrUpdateAsync(entities: identities, bulkConfig: bulkConfig, cancellationToken: ct);
    }
}
