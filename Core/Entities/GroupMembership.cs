namespace Core.Entities;

/// <summary>Membership link table (many‑to‑many) between Identity and Group.</summary>
public sealed record GroupMembership : GuidEntity
{
    public GroupMembership() { }
    
    public GroupMembership(Guid id) : base(id) { }
}