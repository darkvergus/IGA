namespace Ingestion.Mapping;

public class ImportMapping
{
    public Guid Id { get; set; }
    public string EntityClrName { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<ImportMappingItem> Items { get; set; } = new List<ImportMappingItem>();
}