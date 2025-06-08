using System.Text.Json;
using Core.Domain.Dynamic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Converter;

public sealed class DictionaryJsonConverter() : ValueConverter<IDictionary<string, DynamicAttributeValue>, string>(
    v => JsonSerializer.Serialize(v, JsonOpts),
    v => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(v, JsonOpts)!)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    /// <summary>
    /// Ensures EF Core can detect changes inside the dictionary.
    /// Without this, EF won’t realise something changed unless the *reference* changed.
    /// </summary>
    public static readonly ValueComparer<IDictionary<string, DynamicAttributeValue>> Comparer
        = new((a, b) => JsonSerializer.Serialize(a, JsonOpts) == JsonSerializer.Serialize(b, JsonOpts),
            v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(
                JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!);
}