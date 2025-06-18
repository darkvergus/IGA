using Domain.Core.Entities.Provision;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class ProvisionerCfg : IEntityTypeConfiguration<Provisioner>
{
    public void Configure(EntityTypeBuilder<Provisioner> builder)
    {
        builder.ToTable("Provisioner");
        builder.HasKey(config => config.Id);
        builder.Property(config => config.Id).HasColumnName("Id");
        builder.Property(config => config.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(config => config.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
        builder.Property(config => config.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(config => config.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(config => config.Version).HasColumnName("Version");

        builder.HasMany(config => config.Instances)
            .WithOne(instance => instance.Provisioner)
            .HasForeignKey(instance => instance.ProvisionerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(new Provisioner
        {
            Id = 1, Name = "LDAPProvisioner", IsEnabled = true, Version = "1.0.0", CreatedAt = new(2025, 6, 18, 0, 0, 0, DateTimeKind.Utc), 
            ModifiedAt = null, Instances = null
        });
    }
}