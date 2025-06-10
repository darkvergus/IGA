using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using Core.Domain.Dynamic;
using Core.Domain.Interfaces;
using Ingestion.Interfaces;
using Ingestion.Mapping;

namespace Ingestion.Internal;

/// <summary>
/// Builds (and caches) an <see cref="IRowMapper"/> that converts one raw record
/// into the requested domain entity, based on a stored <see cref="ImportMapping"/>.
/// </summary>
internal static class RowMapperFactory
{
    private static readonly ConcurrentDictionary<(Type, Guid), IRowMapper> Cache = new();

    public static IRowMapper Get(Type entityType, ImportMapping mapping) => Cache.GetOrAdd((entityType, mapping.Id), _ => Build(entityType, mapping));

    private static IRowMapper Build(Type entityType, ImportMapping map)
    {
        ConstructorInfo ctor = entityType.GetConstructor([typeof(Guid)]) ?? throw new InvalidOperationException($"{entityType.Name} must expose ctor(Guid)");

        Dictionary<string, PropertyInfo> propsByName = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        bool supportsDyn = typeof(IHasDynamicAttributes).IsAssignableFrom(entityType);

        ImportMappingItem[] items = map.Items.ToArray();

        return new Mapper(ctor, propsByName, items, supportsDyn);
    }
    private sealed class Mapper(ConstructorInfo ctor, Dictionary<string, PropertyInfo> props, ImportMappingItem[] items, bool supportsDynamic) : IRowMapper
    {
        private readonly Func<object, object?> getValue = BuildValueAccessor();

        /// Fast accessor turning `object rawRecord` (ExpandoObject or IDictionary)
        /// into a lookup by string column name.
        private static Func<object, object?> BuildValueAccessor()
        {
            return raw =>
            {
                if (raw is IDictionary<string, object> dict)
                {
                    return dict;
                }

                if (raw is ExpandoObject eo)
                {
                    return eo;
                }

                throw new InvalidOperationException($"Unsupported record type {raw.GetType().Name}");
            };
        }

        public object Convert(object rawRecord)
        {
            if (getValue(rawRecord) is not IDictionary<string, object> row)
            {
                throw new FormatException("Raw record is not a column dictionary");
            }
            
            object entity = ctor.Invoke([Guid.NewGuid()]);
            
            foreach (ImportMappingItem importMappingItem in items)
            {
                if (!row.TryGetValue(importMappingItem.SourceColumn, out object? valObj))
                {
                    throw new FormatException($"Missing column '{importMappingItem.SourceColumn}'");
                }

                string raw = valObj?.ToString() ?? string.Empty;
                
                if (props.TryGetValue(importMappingItem.TargetAttribute, out PropertyInfo? p))
                {
                    var converted = Convert.ChangeType(raw, p.PropertyType);
                    p.SetValue(entity, converted);

                    continue;
                }
                
                if (supportsDynamic)
                {
                    IHasDynamicAttributes dynamicAttributes = (IHasDynamicAttributes)entity;
                    dynamicAttributes.Attributes[importMappingItem.TargetAttribute] = DynamicAttributeValue.From(raw);

                    continue;
                }

                throw new InvalidOperationException($"Cannot map {importMappingItem.SourceColumn} → {importMappingItem.TargetAttribute} on {entity.GetType().Name}");
            }

            return entity!;
        }
    }
}