using System.Text.Json;
using Core.Domain.Dynamic;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using ZLinq;

namespace Core.Domain.Extensions;

public static class DynamicAttributeExtensions
{
    public static T? GetAttr<T>(this IHasDynamicAttributes owner, string sysName)
    {
        DynamicAttributeValue? dynamicAttributeValue = owner.Attributes.AsValueEnumerable().FirstOrDefault(attributeValue
            => attributeValue.Definition?.SystemName == sysName);
        return dynamicAttributeValue is null ? default : dynamicAttributeValue.As<T>();
    }

    public static void SetAttr<T>(this IHasDynamicAttributes owner, DynamicAttributeDefinition definition, T? value)
    {
        DynamicAttributeValue? dynamicAttributeValue = owner.Attributes.AsValueEnumerable().FirstOrDefault(attributeValue
            => attributeValue.DefinitionId == definition.Id);

        if (dynamicAttributeValue is null)
        {
            dynamicAttributeValue = DynamicAttributeValue.From(definition.Id, ((Entity<Guid>)owner).Id, value);
            owner.Attributes.Add(dynamicAttributeValue);
        }
        else
        {
            dynamicAttributeValue.JsonValue = value is null ? null : JsonSerializer.Serialize(value);
        }
    }
    
    /// <summary>
    /// Update an existing attribute by <c>SystemName</c>.
    /// Throws if the value row doesn’t exist on the entity.
    /// </summary>
    public static void SetAttr<T>(this IHasDynamicAttributes owner, string sysName, T? value)
    {
        DynamicAttributeValue? attributeValue = owner.Attributes.FirstOrDefault(dynamicAttributeValue => dynamicAttributeValue.Definition?.SystemName == sysName);

        if (attributeValue is null)
        {
            throw new InvalidOperationException($"Dynamic attribute “{sysName}” is not loaded for this entity.");
        }

        attributeValue.JsonValue = value is null ? null : JsonSerializer.Serialize(value);
    }
}