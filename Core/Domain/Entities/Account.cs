namespace Core.Domain.Entities;

/// <summary>
/// Representation of an Account.
/// </summary>
public sealed partial record Account : GuidEntity
{
    public Account() { }
    
    public Account(Guid id) : base(id) { }
}