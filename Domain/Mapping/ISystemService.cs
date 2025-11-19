using Core.Entities;

namespace Domain.Mapping;

public interface ISystemService
{
    Task<SystemConfiguration> CreateAsync(SystemConfiguration configuration, CancellationToken cancellationToken);
    Task<SystemConfiguration?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SystemConfiguration>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(SystemConfiguration configuration, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}