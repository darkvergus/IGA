using Core.Domain.Records;

namespace Core.Domain.Interfaces;

/// <summary>
/// Marker interface for entities that carry dynamic attributes.
/// </summary>
public interface IHasDynamicAttributes
{
    IDictionary<string, DynamicAttributeValue> Attributes { get; init; }
}