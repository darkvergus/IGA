using Core.Enums;

namespace Core.Dynamic;

/// <summary>
/// Definition of a custom attribute that administrators create via the website.
/// Example: Name="firstName", TargetEntity="Identity", DataType=String.
/// </summary>
public sealed record DynamicAttributeDefinition(
    Guid Id,
    string DisplayName,
    string SystemName,
    AttributeDataType DataType,
    Type? TargetEntity = null,
    KeyType KeyType = KeyType.Int,
    int? MaxLength = null,
    bool IsRequired = false,
    string? Description = null);