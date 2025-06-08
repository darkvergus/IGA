namespace Core.Domain.Entities;

/// <summary>Membership link table (many‑to‑many) between Identity and Group.</summary>
public sealed record GroupMembership() : Entity<Guid>(Guid.NewGuid());