using System.Reflection;

namespace Ingestion.Extensions;

public static class ReflectionExtensions
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropCache = new();

    public static KeyValuePair<string, string>[] ToKeyValuePairs(this object obj)
    {
        Type type = obj.GetType();
        if (!PropCache.TryGetValue(type, out PropertyInfo[]? props))
        {
            props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropCache[type] = props;
        }

        KeyValuePair<string, string>[] list = new KeyValuePair<string, string>[props.Length];
        for (int i = 0; i < props.Length; i++)
        {
            PropertyInfo propertyInfo = props[i];
            string value = propertyInfo.GetValue(obj)?.ToString() ?? string.Empty;
            list[i] = new KeyValuePair<string, string>(propertyInfo.Name, value);
        }
        return list;
    }
}