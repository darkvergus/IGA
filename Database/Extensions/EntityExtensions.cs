using Core.Domain.Entities;
using Core.Domain.Extensions;
using Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Extensions;

public static class EntityExtensions
{
    /// <summary>
    /// Reads a Guid stored in a dynamic attribute (sysName) and loads the corresponding entity T from the DbContext.
    /// Returns null if the attribute is missing or the entity isn’t found.
    /// </summary>
    public static async Task<T?> GetEntityRefAsync<T>(this IHasDynamicAttributes owner, DbContext db, string sysName, CancellationToken cancellationToken = default)
        where T : class
    {
        Guid? id = owner.GetAttr<Guid?>(sysName);
        if (id is null)
        {
            return null;
        }
        
        return await db.Set<T>().FindAsync([id.Value], cancellationToken).AsTask();
    }

    /// <summary>
    /// Stores a reference to an EF entity by its Id into a dynamic attribute (sysName).
    /// </summary>
    public static void SetEntityRef<T>(this IHasDynamicAttributes owner, string sysName, T? entity) 
        where T : class
    {
        Guid? id = entity is not null && entity is Entity<Guid> e ? e.Id : null;

        owner.SetAttr(sysName, id);
    }
}