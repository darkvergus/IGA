using System.Reflection;
using Core.Domain.Dynamic;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Ingestion.Mapping;

namespace Ingestion.Services;

public class RowMapper
{
    private readonly Dictionary<string, Guid> nameToId;
    private readonly Dictionary<string, AttributeDataType> typeLookup;
    private readonly Dictionary<string, DynamicAttributeDefinition> defs;

    public RowMapper(IEnumerable<DynamicAttributeDefinition> defs)
    {
        this.defs = defs.ToDictionary(d => d.SystemName, StringComparer.OrdinalIgnoreCase);
        nameToId = defs.ToDictionary(d => d.SystemName, d => d.Id, StringComparer.OrdinalIgnoreCase);
        typeLookup = defs.ToDictionary(d => d.SystemName, d => d.DataType, StringComparer.OrdinalIgnoreCase);
    }

    public object MapRow(Type targetType, Guid entityId, IDictionary<string, string> row, ImportMapping map)
    {
        object entity = Activator.CreateInstance(targetType, entityId) ?? throw new InvalidOperationException($"Failed to create {targetType.Name}");

        if (entity is not IHasDynamicAttributes hasDyn)
        {
            throw new InvalidOperationException($"{targetType.Name} must implement IHasDynamicAttributes");
        }

        foreach (ImportMappingItem item in map.FieldMappings)
        {
            if (!row.TryGetValue(item.SourceFieldName, out string? raw))
            {
                continue;
            }

            if (!nameToId.TryGetValue(item.TargetFieldName, out Guid defId))
            {
                throw new InvalidOperationException($"Unknown dynamic attribute {item.TargetFieldName}");
            }

            DynamicAttributeDefinition definition = defs[item.TargetFieldName];
            AttributeDataType type = typeLookup[item.TargetFieldName];
            object? value = ParseValue(raw, type);

            DynamicAttributeValue dynamicAttributeValue = DynamicAttributeValue.From(defId, entityId, value);
            dynamicAttributeValue.Definition = definition;

            hasDyn.Attributes.Add(dynamicAttributeValue);
            
            PropertyInfo? propertyInfo = targetType.GetProperty(item.TargetFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is { CanWrite: true })
            {
                propertyInfo.SetValue(entity, value);
            }
        }

        return entity;
    }

    private static object? ParseValue(string raw, AttributeDataType type) =>
        type switch
        {
            AttributeDataType.Int => int.TryParse(raw, out int i) ? i : null,
            AttributeDataType.Decimal => decimal.TryParse(raw, out decimal d) ? d : null,
            AttributeDataType.Bool => bool.TryParse(raw, out bool b) ? b : null,
            AttributeDataType.DateTime => DateTime.TryParse(raw, out DateTime dt) ? dt : null,
            AttributeDataType.Guid => Guid.TryParse(raw, out Guid g) ? g : null,
            _ => raw
        };
}