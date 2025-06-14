using Core.Dynamic;

namespace Web.DTO;

public sealed record ProvisioningRequestDto(Guid AccountId, Guid ResourceId, string ExternalId, IReadOnlyDictionary<string, DynamicAttributeValue>? Delta);
