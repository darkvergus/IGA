using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// Specific privilege (e.g., role, AD group) on a Resource.
/// </summary>
public sealed record Entitlement(Guid Id, Guid ResourceId) : Entity<Guid>(Id), IHasDynamicAttributes
{
    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}