using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// Logical collection of Accounts or Entitlements.
/// </summary>
public sealed record Group(Guid Id, Guid ResourceId) : Entity<Guid>(Id), IHasDynamicAttributes
{
    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}