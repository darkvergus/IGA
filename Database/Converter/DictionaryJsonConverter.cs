using System.Text.Json;
using Core.Domain.Dynamic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Converter;

public sealed class DictionaryJsonConverter() : ValueConverter<IDictionary<string, DynamicAttributeValue>, string>(
    dynamicAttributeValues => JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts),
    json => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(json, JsonOpts)!)
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
        = new((dynamicAttributeValues, dictionary) => JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts) == JsonSerializer.Serialize(dictionary, JsonOpts),
            dynamicAttributeValues => JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts).GetHashCode(),
            dynamicAttributeValues => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(
                JsonSerializer.Serialize(dynamicAttributeValues, JsonOpts), JsonOpts)!);
}