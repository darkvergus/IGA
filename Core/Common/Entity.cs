using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Common;

/// <summary>
/// Base entity providing immutable identity and audit metadata.
/// </summary>
public abstract record Entity<TId>(TId Id) : IEntity
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    [ConcurrencyCheck] public int Version { get; set; }
    public ulong AttrHash { get; set; }
}