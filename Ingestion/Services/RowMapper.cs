using System.Buffers;
using System.Reflection;
using System.IO.Hashing;
using System.Text;
using Core.Domain.Dynamic;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Ingestion.Mapping;
using ZLinq;

namespace Ingestion.Services;

public class RowMapper
{
    private readonly Dictionary<string, Guid> nameToId;
    private readonly Dictionary<string, AttributeDataType> typeLookup;
    private readonly Dictionary<string, DynamicAttributeDefinition> defs;

    public RowMapper(IEnumerable<DynamicAttributeDefinition> defs)
    {
        this.defs = defs.ToDictionary(attributeDefinition => attributeDefinition.SystemName, StringComparer.OrdinalIgnoreCase);
        nameToId = defs.ToDictionary(attributeDefinition => attributeDefinition.SystemName, attributeDefinition => attributeDefinition.Id, StringComparer.OrdinalIgnoreCase);
        typeLookup = defs.ToDictionary(attributeDefinition => attributeDefinition.SystemName, attributeDefinition => attributeDefinition.DataType, StringComparer.OrdinalIgnoreCase);
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

            if (!nameToId.TryGetValue(item.TargetFieldName, out Guid definitionId))
            {
                throw new InvalidOperationException($"Unknown dynamic attribute {item.TargetFieldName}");
            }

            DynamicAttributeDefinition definition = defs[item.TargetFieldName];
            AttributeDataType type = typeLookup[item.TargetFieldName];
            object? value = ParseValue(raw, type);

            DynamicAttributeValue dynamicAttributeValue = DynamicAttributeValue.From(definitionId, entityId, value);
            dynamicAttributeValue.Definition = definition;

            hasDyn.Attributes.Add(dynamicAttributeValue);
            
            PropertyInfo? propertyInfo = targetType.GetProperty(item.TargetFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is { CanWrite: true })
            {
                propertyInfo.SetValue(entity, value);
            }
        }
        
        ulong bagHash = ComputeBagHash(hasDyn.Attributes);
        targetType.GetProperty("AttrHash")!.SetValue(entity, bagHash); 
        
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
    
    private static ulong ComputeBagHash(IEnumerable<DynamicAttributeValue> bag)
    {
        XxHash64 hash = new();
        Span<byte> sep = [0];

        foreach (DynamicAttributeValue v in bag.AsValueEnumerable().OrderBy(b => b.Definition!.SystemName, StringComparer.Ordinal))
        {
            AppendUtf8(v.Definition!.SystemName, ref hash);
            hash.Append(sep);
            
            if (v.JsonValue is { } js)
            {
                AppendUtf8(js, ref hash);
            }

            hash.Append(sep);
        }

        Span<byte> hash8 = stackalloc byte[8];
        hash.GetCurrentHash(hash8);
        return BitConverter.ToUInt64(hash8);

        static void AppendUtf8(string s, ref XxHash64 hash)
        {
            int max = Encoding.UTF8.GetMaxByteCount(s.Length);
            if (max <= 1024)
            {
                Span<byte> tmp = stackalloc byte[max];
                int used = Encoding.UTF8.GetBytes(s, tmp);
                hash.Append(tmp[..used]);
            }
            else
            {
                byte[] rented = ArrayPool<byte>.Shared.Rent(max);
                try
                {
                    int used = Encoding.UTF8.GetBytes(s, rented);
                    hash.Append(rented.AsSpan(0, used));
                }
                finally { ArrayPool<byte>.Shared.Return(rented); }
            }
        }
    }
}