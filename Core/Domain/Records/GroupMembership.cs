namespace Core.Domain.Records;

/// <summary>Membership link table (many‑to‑many) between Identity and Group.</summary>
public sealed record GroupMembership(Guid IdentityId, Guid GroupId) : Entity<Guid>(Guid.NewGuid());