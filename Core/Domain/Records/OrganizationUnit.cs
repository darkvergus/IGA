using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// Logical collection for the departments.
/// </summary>
/// <param name="Id"></param>
public record OrganizationUnit(Guid Id) : Entity<Guid>(Id), IHasDynamicAttributes
{
    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}