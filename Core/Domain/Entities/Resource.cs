namespace Core.Domain.Entities;

/// <summary>
/// External system or application integrated with IGA.
/// </summary>
public sealed record Resource : GuidEntity
{
    public Resource() { }
    
    public Resource(Guid id) : base(id) { }
}
