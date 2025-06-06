using System.ComponentModel.DataAnnotations;
using Core.Domain.Enums;
using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// Canonical person. Properties like firstName/lastName are stored in Attributes.
/// </summary>
public sealed record Identity(Guid Id) : Entity<Guid>(Id), IHasDynamicAttributes
{
    public IdentityCategoryType Category { get; set; } = IdentityCategoryType.Unknown;
    public IdentityStatusType Status { get; set; } = IdentityStatusType.Active;

    [Required]
    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}