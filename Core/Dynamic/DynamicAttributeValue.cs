using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Dynamic;

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

    public ulong AttrHash { get; set; }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DynamicAttributeValue From<T>(Guid definitionId, Guid entityId, T? value)
    {
        string? json;

        switch (value)
        {
            case null:
                json = null;

                break;
            case string s:
                json = s;

                break;
            case ISpanFormattable formattable:
            {
                Span<char> tmp = stackalloc char[32];
                json = formattable.TryFormat(tmp, out int written, default, null) ? new(tmp[..written]) : formattable.ToString(null, null);

                break;
            }

            default:
                json = JsonSerializer.Serialize(value, JsonOpts);

                break;
        }

        ulong hash = XxHash64(json ?? string.Empty);

        return new()
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId,
            EntityId = entityId,
            JsonValue = json,
            AttrHash = hash
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DynamicAttributeValue From<T>(Guid definitionId, Guid? entityId, T? value) => From(definitionId, entityId ?? Guid.Empty, value);

    public T? As<T>()
    {
        if (JsonValue is null)
        {
            return default;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)(JsonValue.Length > 0 && JsonValue[0] != '"' ? JsonValue : JsonSerializer.Deserialize<string>(JsonValue, JsonOpts)!);
            }

            return JsonSerializer.Deserialize<T>(JsonValue, JsonOpts);
        }
        catch (JsonException)
        {
            if (typeof(T) == typeof(Guid) && Guid.TryParse(JsonValue, out Guid g))
            {
                return (T)(object)g;
            }

            if (typeof(T) == typeof(int)  && int.TryParse(JsonValue, out int i))
            {
                return (T)(object)i;
            }

            if (typeof(T) == typeof(decimal) && decimal.TryParse(JsonValue, out decimal d))
            {
                return (T)(object)d;
            }

            if (typeof(T) == typeof(bool) && bool.TryParse(JsonValue, out bool b))
            {
                return (T)(object)b;
            }
            
            if (typeof(T) == typeof(DateTime) && DateTime.TryParse(JsonValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            {
                return (T)(object)dt;
            }

            if (typeof(T) == typeof(DateTimeOffset) && DateTimeOffset.TryParse(JsonValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dto))
            {
                return (T)(object)dto;
            }

            throw;
        }
    }

    private static ulong XxHash64(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        byte[] hashBytes = System.IO.Hashing.XxHash64.Hash(System.Text.Encoding.UTF8.GetBytes(value));

        return BitConverter.ToUInt64(hashBytes, 0);
    }
}