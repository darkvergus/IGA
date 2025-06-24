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
                switch (item.Type)
                {
                    case MappingFieldType.Map:
                        if (row.TryGetValue(item.SourceFieldName, out string? raw))
                        {
                            WriteScalar(entity, targetType, entityId, dynamicAttributes, item.TargetFieldName, raw);
                        }

                        break;
                    case MappingFieldType.Constant:
                        WriteScalar(entity, targetType, entityId, dynamicAttributes, item.TargetFieldName, item.SourceFieldName);
                        break;    
                    case MappingFieldType.Expression:
                        string exprResult = EvaluateExpressionStub(item.SourceFieldName, row);
                        WriteScalar(entity, targetType, entityId, dynamicAttributes,
                            item.TargetFieldName, exprResult);
                        break;
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

        foreach (DynamicAttributeValue value in bag.AsValueEnumerable().OrderBy(attributeValue => attributeValue.Definition!.SystemName, StringComparer.Ordinal))
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
    
    private void WriteScalar(object entity, Type targetType, Guid entityId, IHasDynamicAttributes dynamicAttributes, string targetName, string raw)
    {
        if (GetCachedProperty(targetType, targetName) is { CanWrite: true } prop &&
            TypeToAttrTypeExtensions.ParseValue(raw, prop.PropertyType.ToAttrType()) is { } scalar)
        {
            GetSetter(prop)(entity, scalar);
        }

        if (nameToId.TryGetValue(targetName, out Guid defId))
        {
            AttributeDataType dataType = typeLookup[targetName];
            object? value = TypeToAttrTypeExtensions.ParseValue(raw, dataType);

            DynamicAttributeValue attributeValue = DynamicAttributeValue.From(defId, entityId, value);
            attributeValue.Definition = definitions[targetName];
            dynamicAttributes.Attributes.Add(attributeValue);
        }
    }
    
    private static string EvaluateExpressionStub(string expr, IDictionary<string,string> row)
    {
        // TODO: replace with proper dynamic evaluator.
        // For now return the raw expression string so you can see it's wired through.
        return expr;
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