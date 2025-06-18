using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Hashing;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Core.Dynamic;
using Core.Enums;
using Core.Interfaces;
using Domain.Extensions;
using Domain.Mapping;
using ZLinq;

namespace Domain.Services;

public class RowMapper
{
    private readonly Dictionary<string, Guid> nameToId;
    private readonly Dictionary<string, AttributeDataType> typeLookup;
    private readonly Dictionary<string, DynamicAttributeDefinition> definitions;

    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> SetterCache = new();

    public RowMapper(IEnumerable<DynamicAttributeDefinition> definitions)
    {
        this.definitions = definitions.ToDictionary(attributeDefinition => attributeDefinition.SystemName, StringComparer.OrdinalIgnoreCase);
        nameToId = definitions.ToDictionary(attributeDefinition => attributeDefinition.SystemName, attributeDefinition => attributeDefinition.Id,
            StringComparer.OrdinalIgnoreCase);
        typeLookup = definitions.ToDictionary(attributeDefinition => attributeDefinition.SystemName, attributeDefinition => attributeDefinition.DataType,
            StringComparer.OrdinalIgnoreCase);
    }

    public object MapRow(Type targetType, Guid entityId, IDictionary<string, string> row, ImportMapping? map)
    {
        object entity = Activator.CreateInstance(targetType, entityId) ?? throw new InvalidOperationException($"Failed to create {targetType.Name}");

        if (entity is not IHasDynamicAttributes dynamicAttributes)
        {
            throw new InvalidOperationException($"{targetType.Name} must implement IHasDynamicAttributes");
        }

        if (map != null)
        {
            foreach (ImportMappingItem item in map.FieldMappings)
            {
                if (!row.ContainsKey(item.SourceFieldName))
                {
                    Console.WriteLine($"[DEBUG] key '{item.SourceFieldName}' not found - row keys: {string.Join(", ", row.Keys)}");
                }

                if (!row.TryGetValue(item.SourceFieldName, out string? raw)) { continue; }

                PropertyInfo? prop = GetCachedProperty(targetType, item.TargetFieldName);
                if (prop is { CanWrite: true })
                {
                    object? scalar = TypeToAttrTypeExtensions.ParseValue(raw, prop.PropertyType.ToAttrType());

                    if (scalar != null)
                    {
                        GetSetter(prop)(entity, scalar);
                    }
                }

                if (nameToId.TryGetValue(item.TargetFieldName, out Guid defId))
                {
                    AttributeDataType dtype = typeLookup[item.TargetFieldName];
                    object? value = TypeToAttrTypeExtensions.ParseValue(raw, dtype);

                    DynamicAttributeValue attributeValue = DynamicAttributeValue.From(defId, entityId, value);
                    attributeValue.Definition = definitions[item.TargetFieldName];
                    dynamicAttributes.Attributes.Add(attributeValue);
                }
            }
        }

        ulong bagHash = ComputeBagHash(dynamicAttributes.Attributes);
        targetType.GetProperty("AttrHash")!.SetValue(entity, bagHash);

        return entity;
    }

    private static ulong ComputeBagHash(IEnumerable<DynamicAttributeValue> bag)
    {
        XxHash64 hash = new();
        Span<byte> sep = [0];

        foreach (DynamicAttributeValue value in
                 bag.AsValueEnumerable().OrderBy(attributeValue => attributeValue.Definition!.SystemName, StringComparer.Ordinal))
        {
            AppendUtf8(value.Definition!.SystemName, ref hash);
            hash.Append(sep);

            if (value.JsonValue is { } js)
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
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }

    private static PropertyInfo? GetCachedProperty(Type type, string propName) => PropCache.GetOrAdd((type, propName), key => key.Item1.GetProperty(key.Item2,
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));

    private static Action<object, object> GetSetter(PropertyInfo propertyInfo)
    {
        return SetterCache.GetOrAdd(propertyInfo, prop =>
        {
            ParameterExpression objParam = Expression.Parameter(typeof(object), "obj");
            ParameterExpression valParam = Expression.Parameter(typeof(object), "val");
            BinaryExpression body = Expression.Assign(Expression.Property(Expression.Convert(objParam, prop.DeclaringType!), prop),
                Expression.Convert(valParam, prop.PropertyType));

            return Expression.Lambda<Action<object, object>>(body, objParam, valParam).Compile();
        });
    }
}