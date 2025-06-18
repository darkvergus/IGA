using Core.Common;
using Core.Dynamic;
using Core.Entities;
using Database.Converter;
using Database.Extensions;
using Domain.Core.Entities;
using Domain.Core.Entities.Connector;
using Domain.Core.Entities.Provision;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using ZLinq;

namespace Database.Context;

public sealed class IgaDbContext(DbContextOptions<IgaDbContext> options) : DbContext(options)
{
    public DbSet<Identity> Identities => Set<Identity>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<DynamicAttributeDefinition> DynamicAttributeDefinitions { get; set; }
    public DbSet<Connector> ConnectorConfigs { get; set; }
    public DbSet<Provisioner> ProvisionConfigs { get; set; }
    public DbSet<ProvisionerInstance> ProvisionerInstances { get; set; }
    public DbSet<InstanceSetting> InstanceSettings { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) => configurationBuilder.UseIgaModel();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (IMutableProperty prop in modelBuilder.Model.GetEntityTypes().AsValueEnumerable().SelectMany(entity => entity.GetProperties()
                     .AsValueEnumerable().Where(property => property.ClrType == typeof(IDictionary<string, DynamicAttributeValue>))))
        {
            prop.SetValueComparer(DictionaryJsonConverter.Comparer);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IgaDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess = true)
    {
        IncrementVersions();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        IncrementVersions();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void IncrementVersions()
    {
        foreach (EntityEntry<Entity<Guid>> entry in ChangeTracker.Entries<Entity<Guid>>().AsValueEnumerable()
                     .Where(entry => entry.State == EntityState.Modified))
        {
            entry.Entity.Version += 1;
        }
    }
}