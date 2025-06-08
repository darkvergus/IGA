using Core.Domain.Dynamic;
using Core.Domain.Entities;
using Database.Converter;
using Database.Extensions;
using Microsoft.EntityFrameworkCore;
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) => configurationBuilder.UseIgaModel();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (IMutableProperty? prop in modelBuilder.Model.GetEntityTypes().AsValueEnumerable().SelectMany(entity => entity.GetProperties()
        .Where(p => p.ClrType == typeof(IDictionary<string, DynamicAttributeValue>))))
        {
            prop.SetValueComparer(DictionaryJsonConverter.Comparer);
        }
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IgaDbContext).Assembly);
    }
}