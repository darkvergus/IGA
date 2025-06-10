namespace Ingestion.Mapping;

public class ImportMappingItem
{
    public Guid Id { get; set; }
    public Guid ImportMappingId { get; set; }
    public string SourceColumn { get; set; } = null!;
    public string TargetAttribute { get; set; } = null!;
    public string? DataType { get; set; }
    public bool Required { get; set; }
    public bool Unique { get; set; }
}