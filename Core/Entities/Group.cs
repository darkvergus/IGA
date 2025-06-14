namespace Core.Entities;

/// <summary>
/// Logical collection of Accounts or Entitlements.
/// </summary>
public sealed record Group : GuidEntity
{
    public Group() { }
    
    public Group(Guid id) : base(id) { }
}