using Core.Domain.Enums;

namespace Core.Domain.Dynamic;

/// <summary>
/// Definition of a custom attribute that administrators create via the website.
/// Example: Name="firstName", TargetEntity="Identity", DataType=String.
/// </summary>
public sealed record DynamicAttributeDefinition(
    Guid Id,
    string DisplayName,
    string SystemName,
    AttributeDataType DataType,
    string TargetEntity,
    int? MaxLength = null,
    bool IsRequired = false,
    string? Description = null);