namespace Core.Domain.Enums;

/// <summary>
/// Primitive data types we support for custom attributes. Keep this small so we can map
/// to JSON / SQL easily.
/// </summary>
public enum AttributeDataType
{
    String,
    Int,
    Decimal,
    Bool,
    DateTime,
    Guid,
    Binary,
    Json,
    Set,
    Reference
}