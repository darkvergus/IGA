using Core.Domain.Dynamic;

namespace Web.DTO;

public sealed record ProvisioningRequestDto(Guid AccountId, Guid GroupId, string ExternalId, IReadOnlyDictionary<string, DynamicAttributeValue>? Delta);
