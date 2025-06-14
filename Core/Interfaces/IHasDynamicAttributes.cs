using Core.Dynamic;

namespace Core.Interfaces;

/// <summary>
/// Marker interface for entities that carry dynamic attributes.
/// </summary>
public interface IHasDynamicAttributes
{
    ICollection<DynamicAttributeValue> Attributes { get; }
}