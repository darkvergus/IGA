namespace Core.Domain.Entities;

/// <summary>
/// Canonical person. Properties like firstName/lastName are stored in Attributes.
/// </summary>
public sealed partial record Identity : GuidEntity
{
    public Identity() { }
    
    public Identity(Guid id) : base(id) { }
}