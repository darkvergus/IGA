using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Domain.Dynamic;

/// <summary>
/// Actual value for a custom attribute on an entity instance.
/// </summary>
/// <summary>
/// A single polymorphic attribute value that can be serialized as JSON.
/// </summary>
public sealed record DynamicAttributeValue
{
    public Guid Id { get; init; }
    public Guid DefinitionId { get; init; }
    public Guid EntityId { get; init; }
    
    public DynamicAttributeDefinition? Definition { get; init; }
    
    /// <summary>
    /// Value stored as JSON so we can deserialize according to DataType.
    /// </summary>
    public string? JsonValue { get; set; }

    [JsonIgnore] public bool IsNull => JsonValue is null;

    public static DynamicAttributeValue From<T>(Guid definitionId, Guid entityId, T? value)
    {
        DynamicAttributeValue from = new()
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId,
            EntityId = entityId,
            JsonValue = value is null ? null : JsonSerializer.Serialize(value)
        };

        return from;
    }

    public T? As<T>() => IsNull ? default : JsonSerializer.Deserialize<T>(JsonValue!);
}