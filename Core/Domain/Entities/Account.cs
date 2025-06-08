using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Representation of an Account.
/// </summary>
public sealed partial record Account() : Entity<Guid>(Guid.NewGuid()), IHasDynamicAttributes
{
    public ICollection<DynamicAttributeValue> Attributes { get; init; } = new List<DynamicAttributeValue>();
}