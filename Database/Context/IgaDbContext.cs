using System.Text.Json;
using Core.Domain.Records;
using Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Context;

public sealed class IgaDbContext(DbContextOptions<IgaDbContext> options) : DbContext(options)
{
    public DbSet<Identity> Identities => Set<Identity>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private static readonly ValueConverter<Dictionary<string, DynamicAttributeValue>, string> AttrConverter =
        new(values => JsonSerializer.Serialize(values, JsonOpts),
            json => JsonSerializer.Deserialize<Dictionary<string, DynamicAttributeValue>>(json, JsonOpts) ?? new());

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) => configurationBuilder.UseIgaModel();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdentity(modelBuilder);
        ConfigureGroup(modelBuilder);
        ConfigureResource(modelBuilder);
        ConfigureEntitlement(modelBuilder);
        ConfigureAccount(modelBuilder);
        ConfigureOrganizations(modelBuilder);
    }
    
    private void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Identity>(builder =>
        {
            builder.HasKey(identity => identity.Id);
            builder.Property(identity => identity.Category).HasConversion<int>().IsRequired();
            builder.Property(identity => identity.Status).HasConversion<int>().IsRequired();
            builder.Property(identity => identity.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            builder.Property(identity => identity.CreatedAt).IsRequired();
            builder.Property(identity => identity.ModifiedAt).IsRequired();
        });
    }

    private void ConfigureGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(builder =>
        {
            builder.HasKey(group => group.Id);
            builder.Property(group => group.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });
    }

    private void ConfigureResource(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(builder =>
        {
            builder.HasKey(resource => resource.Id);
            builder.Property(resource => resource.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });
    }

    private void ConfigureEntitlement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entitlement>(builder =>
        {
            builder.HasKey(entitlement => entitlement.Id);
            builder.Property(entitlement => entitlement.ResourceId).IsRequired();
            builder.HasIndex(entitlement => entitlement.ResourceId);
            builder.Property(entitlement => entitlement.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });
    }

    private void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(builder =>
        {
            builder.HasKey(account => account.Id);
            builder.Property(account => account.IdentityId).IsRequired();
            builder.Property(account => account.ResourceId).IsRequired();
            builder.HasIndex(account => new {account.IdentityId, account.ResourceId}).IsUnique();
            builder.Property(account => account.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });
    }
    
    private void ConfigureOrganizations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationUnit>(builder =>
        {
            builder.HasKey(organizationUnit => organizationUnit.Id);
            builder.Property(organizationUnit => organizationUnit.Attributes)
                .HasConversion(AttrConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            builder.Property(identity => identity.CreatedAt).IsRequired();
            builder.Property(identity => identity.ModifiedAt).IsRequired();
        });
    }
}