using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Canonical person. Properties like firstName/lastName are stored in Attributes.
/// </summary>
public sealed partial record Identity() : Entity<Guid>(Guid.NewGuid()), IHasDynamicAttributes
{
    public ICollection<DynamicAttributeValue> Attributes { get; init; } = new List<DynamicAttributeValue>();
}