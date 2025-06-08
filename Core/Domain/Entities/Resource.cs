using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// External system or application integrated with IGA.
/// </summary>
public sealed record Resource() : Entity<Guid>(Guid.NewGuid()), IHasDynamicAttributes
{
    public ICollection<DynamicAttributeValue> Attributes { get; init; } = new List<DynamicAttributeValue>();
}