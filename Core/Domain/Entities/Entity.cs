using System.ComponentModel.DataAnnotations;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Base entity providing immutable identity and audit metadata.
/// </summary>
public abstract record Entity<TId>(TId Id) : IEntity
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedAt { get; set; }
    [ConcurrencyCheck] public int Version { get; set; }
}