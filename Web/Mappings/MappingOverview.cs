namespace Web.Mappings;

public class MappingOverview
{
    public string Context { get; set; } = default!;
    public string Entity { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string? PrimaryKey { get; set; }
    public int FieldCount { get; set; }
}