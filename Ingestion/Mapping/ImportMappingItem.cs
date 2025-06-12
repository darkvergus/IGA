namespace Ingestion.Mapping;

public class ImportMappingItem(string sourceFieldName, string targetFieldName)
{
    public string SourceFieldName { get; set; } = sourceFieldName;
    public string TargetFieldName { get; set; } = targetFieldName;
}