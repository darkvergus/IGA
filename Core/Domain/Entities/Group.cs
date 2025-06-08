using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Logical collection of Accounts or Entitlements.
/// </summary>
public sealed record Group() : Entity<Guid>(Guid.NewGuid()), IHasDynamicAttributes
{
    public ICollection<DynamicAttributeValue> Attributes { get; init; } = new List<DynamicAttributeValue>();
}