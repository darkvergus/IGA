using Core.Domain.Dynamic;

namespace Core.Domain.Interfaces;

/// <summary>
/// Marker interface for entities that carry dynamic attributes.
/// </summary>
public interface IHasDynamicAttributes
{
    ICollection<DynamicAttributeValue> Attributes { get; }
}