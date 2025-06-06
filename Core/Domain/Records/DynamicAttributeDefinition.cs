using System.ComponentModel.DataAnnotations;
using Core.Domain.Enums;

namespace Core.Domain.Records;

/// <summary>
/// Definition of a custom attribute that administrators create via the website.
/// Example: Name="firstName", TargetEntity="Identity", DataType=String.
/// </summary>
public sealed record DynamicAttributeDefinition(
    Guid Id,
    [property: Required, MaxLength(64)] string Name,
    AttributeDataType DataType,
    [property: Required, MaxLength(64)] string TargetEntity,
    bool IsRequired = false,
    string? Description = null)
{
    public const string TargetIdentity = nameof(Identity);
    public const string TargetAccount = nameof(Account);
    public const string TargetGroup = nameof(Group);
    public const string TargetResource = nameof(Resource);
    public const string TargetEntitlement = nameof(Entitlement);
}