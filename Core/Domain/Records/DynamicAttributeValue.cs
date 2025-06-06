using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Domain.Enums;

namespace Core.Domain.Records;

/// <summary>
/// Actual value for a custom attribute on an entity instance.
/// </summary>
/// <summary>
/// A single polymorphic attribute value that can be serialized as JSON.
/// </summary>
public sealed record DynamicAttributeValue
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeDataType Type { get; init; }

    /// <summary>
    /// The JSON‑encoded representation of the value (or null).
    /// </summary>
    public string? Raw { get; init; }

    [JsonIgnore]
    public bool IsNull => Raw is null;
    
    public static DynamicAttributeValue From<T>(T? value)
    {
        if (value is null)
        {
            return new()
            {
                Type = AttributeDataType.Json, 
                Raw = null
            };
        }

        return new()
        {
            Type = Map(typeof(T)),
            Raw = JsonSerializer.Serialize(value)
        };
    }

    public static DynamicAttributeValue FromString(string value) => new() {Type = AttributeDataType.String, Raw = JsonSerializer.Serialize(value)};

    public T? As<T>() => IsNull ? default : JsonSerializer.Deserialize<T>(Raw!);

    private static AttributeDataType Map(Type t) =>
        t == typeof(string) ? AttributeDataType.String :
        t == typeof(int) || t == typeof(long) ? AttributeDataType.Int :
        t == typeof(bool) ? AttributeDataType.Bool :
        t == typeof(DateTime) || t == typeof(DateTimeOffset) ? AttributeDataType.DateTime :
        t == typeof(Guid) ? AttributeDataType.Guid :
        AttributeDataType.Json;

    public override string ToString() => Raw ?? string.Empty;
}