using Core.Entities;
using Database.Context;
using Domain.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Database.Services;

public sealed class SystemService(IgaDbContext dbContext) : ISystemService
{
    public async Task<SystemConfiguration> CreateAsync(SystemConfiguration configuration, CancellationToken cancellationToken)
    {
        DbSet<SystemConfiguration> set = dbContext.Set<SystemConfiguration>();
        await set.AddAsync(configuration, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return configuration;
    }

    public async Task<SystemConfiguration?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        DbSet<SystemConfiguration> set = dbContext.Set<SystemConfiguration>();
        SystemConfiguration? configuration = await set.FirstOrDefaultAsync(systemConfiguration => systemConfiguration.Id == id, cancellationToken).ConfigureAwait(false);
        return configuration;
    }

    public async Task<IReadOnlyList<SystemConfiguration>> GetAllAsync(CancellationToken cancellationToken)
    {
        DbSet<SystemConfiguration> set = dbContext.Set<SystemConfiguration>();
        List<SystemConfiguration> configurations = await set.OrderBy(systemConfiguration => systemConfiguration.Name).ToListAsync(cancellationToken).ConfigureAwait(false);
        return configurations;
    }

    public async Task UpdateAsync(SystemConfiguration configuration, CancellationToken cancellationToken)
    {
        DbSet<SystemConfiguration> set = dbContext.Set<SystemConfiguration>();
        set.Update(configuration);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        DbSet<SystemConfiguration> set = dbContext.Set<SystemConfiguration>();
        SystemConfiguration? configuration = await set.FirstOrDefaultAsync(systemConfiguration => systemConfiguration.Id == id, cancellationToken).ConfigureAwait(false);

        if (configuration == null)
        {
            return;
        }

        set.Remove(configuration);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}