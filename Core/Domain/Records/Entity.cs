using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Records;

/// <summary>
/// Base entity providing immutable identity and audit metadata.
/// </summary>
public abstract record Entity<TId>(TId Id)
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedAt { get; set; }
    [Timestamp] public byte[]? Version { get; init; }
}