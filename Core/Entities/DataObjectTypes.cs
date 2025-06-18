namespace Core.Entities;

public sealed partial record DataObjectTypes : GuidEntity
{
    public DataObjectTypes() { }
    
    public DataObjectTypes(Guid id) : base(id) { }
}