using System.ComponentModel.DataAnnotations;
using Core.Domain.Interfaces;

namespace Core.Domain.Records;

/// <summary>
/// Representation of an Identity in a given Resource (e.g., AD user, Okta user).
/// </summary>
public sealed record Account(Guid Id, Guid IdentityId, Guid ResourceId) : Entity<Guid>(Id), IHasDynamicAttributes
{
    [Required]
    public Guid IdentityId { get; set; } = IdentityId;
    [Required]
    public Guid ResourceId { get; set; } = ResourceId;
    
    public IDictionary<string, DynamicAttributeValue> Attributes { get; init; } = new Dictionary<string, DynamicAttributeValue>();
}