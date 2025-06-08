using Core.Domain.Dynamic;
using Core.Domain.Interfaces;

namespace Core.Domain.Entities;

/// <summary>
/// Logical collection for the departments.
/// </summary>
/// <param name="Id"></param>
public partial record OrganizationUnit() : Entity<Guid>(Guid.NewGuid()), IHasDynamicAttributes
{
    public ICollection<DynamicAttributeValue> Attributes { get; init; } = new List<DynamicAttributeValue>();
}

