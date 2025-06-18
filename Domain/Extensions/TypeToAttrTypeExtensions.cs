using Core.Enums;

namespace Domain.Extensions;

/// <summary>Maps common CLR types to AttributeDataType.</summary>
public static class TypeToAttrTypeExtensions
{
    public static AttributeDataType ToAttrType(this Type type)
    {
        if (type == typeof(int) || type == typeof(int?))
        {
            return AttributeDataType.Int;
        }

        if (type == typeof(decimal) || type == typeof(decimal?))
        {
            return AttributeDataType.Decimal;
        }

        if (type == typeof(bool) || type == typeof(bool?))
        {
            return AttributeDataType.Bool;
        }

        if (type == typeof(DateTime) || type == typeof(DateTime?))
        {
            return AttributeDataType.DateTime;
        }

        if (type == typeof(Guid) || type == typeof(Guid?))
        {
            return AttributeDataType.Guid;
        }

        return AttributeDataType.String;
    }
    
    public static object? ParseValue(ReadOnlySpan<char> raw, AttributeDataType type) =>
        type switch
        {
            AttributeDataType.Int => int.TryParse(raw, out int i) ? i : null,
            AttributeDataType.Decimal => decimal.TryParse(raw, out decimal d) ? d : null,
            AttributeDataType.Bool => bool.TryParse(raw, out bool b) ? b : null,
            AttributeDataType.DateTime => DateTime.TryParse(raw, out DateTime dt) ? dt : null,
            AttributeDataType.Guid => Guid.TryParse(raw, out Guid g) ? g : null,
            _ => raw.ToString()
        };
}