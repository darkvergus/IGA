namespace Core.Domain.Entities;

/// <summary>
/// Logical collection for the departments.
/// </summary>
/// <param name="Id"></param>
public partial record OrganizationUnit : GuidEntity
{
    public OrganizationUnit() { }
    
    public OrganizationUnit(Guid id) : base(id) { }
}
