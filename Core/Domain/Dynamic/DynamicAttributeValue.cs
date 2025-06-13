using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Domain.Dynamic;


/// <summary>
/// Actual value for a custom attribute on an entity instance.
/// </summary>
/// <summary>
/// A single polymorphic attribute value that can be serialized as JSON.
/// </summary>
[NotMapped]
public sealed record DynamicAttributeValue
{
    public Guid Id { get; init; }
    public Guid DefinitionId { get; init; }
    public Guid EntityId { get; init; }
    
    public DynamicAttributeDefinition? Definition { get; set; }
    
    /// <summary>
    /// Value stored as JSON so we can deserialize according to DataType.
    /// </summary>
    public string? JsonValue { get; set; }

    [JsonIgnore] public bool IsNull => JsonValue is null;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DynamicAttributeValue From<T>(Guid definitionId, Guid entityId, T? value)
    {
        string? jsonValue = value switch
        {
            null => null,
            string s => s,
            int or long or short or double or decimal or bool => value?.ToString(),
            _ => JsonSerializer.Serialize(value, JsonOpts) 
        };
        
        return new DynamicAttributeValue
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId,
            EntityId = entityId,
            JsonValue = jsonValue
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DynamicAttributeValue From<T>(Guid definitionId, Guid? entityId, T? value) => From(definitionId, entityId ?? Guid.Empty, value);

    public T? As<T>() => IsNull ? default : JsonSerializer.Deserialize<T>(JsonValue!);
}