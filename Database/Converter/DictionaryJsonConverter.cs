using System.Text.Json;
using Core.Dynamic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Converter;

public sealed class DictionaryJsonConverter() : ValueConverter<IDictionary<string, DynamicAttributeValue>, string>(
    dynamicAttributeValues => JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts),
    json => SafeDeserialize(json))
{
    private static readonly JsonSerializerOptions JsonOpts;

    static DictionaryJsonConverter() => JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private static Dictionary<string, DynamicAttributeValue> SafeDeserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new(StringComparer.OrdinalIgnoreCase);
            }

            return document.RootElement.Deserialize<Dictionary<string, DynamicAttributeValue>>(JsonOpts) ?? new Dictionary<string, DynamicAttributeValue>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Ensures EF Core can detect changes inside the dictionary.
    /// Without this, EF won’t realise something changed unless the *reference* changed.
    /// </summary>
    public static readonly ValueComparer<IDictionary<string, DynamicAttributeValue>> Comparer
        = new(
            (dynamicAttributeValues, dictionary) =>
                JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts) == JsonSerializer.Serialize(dictionary, JsonOpts),
            dynamicAttributeValues => JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts).GetHashCode(),
            dynamicAttributeValues => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(
                JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts), JsonOpts)!);
}