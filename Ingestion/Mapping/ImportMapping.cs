namespace Ingestion.Mapping;

public class ImportMapping(Type targetType)
{
    public Type TargetEntityType { get; } = targetType;
    public List<ImportMappingItem> FieldMappings { get; init; } = [];
}