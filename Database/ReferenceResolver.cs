using Core.Enums;
using Database.Context;

namespace Database;

public sealed class ReferenceResolver(IgaDbContext dbContext)
{
    public async Task<object?> ResolveAsync(string rawId, KeyType keyType, Type targetClr, CancellationToken cancellationToken = default)
    {
        return keyType switch
        {
            KeyType.Int  when int.TryParse(rawId, out int id)  => await dbContext.FindAsync(targetClr, [id], cancellationToken),
            KeyType.Guid when Guid.TryParse(rawId, out Guid guid) => await dbContext.FindAsync(targetClr, [guid], cancellationToken),
            _ => null
        };
    }

    public bool Validate(string rawId, KeyType keyType)
    {
        return keyType switch
        {
            KeyType.Int  => int.TryParse(rawId, out _),
            KeyType.Guid => Guid.TryParse(rawId, out _),
            _ => false
        };
    }
}