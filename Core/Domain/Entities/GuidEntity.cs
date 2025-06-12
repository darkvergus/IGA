using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Base class for all Guid-key entities that carry dynamic attributes.
/// </summary>
public abstract record GuidEntity : Entity<Guid>, IHasDynamicAttributes, IGuidEntity
{
    protected GuidEntity() : base(Guid.NewGuid()) { }

    protected GuidEntity(Guid id) : base(id) { }
    
    public ICollection<DynamicAttributeValue> Attributes { get; set; } = new List<DynamicAttributeValue>();
}