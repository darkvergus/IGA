using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// External system or application integrated with IGA.
/// </summary>
public sealed record Resource(Guid Id) : Entity<Guid>(Id), IHasDynamicAttributes
{
    public string? ConnectorType { get; init; }

    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}