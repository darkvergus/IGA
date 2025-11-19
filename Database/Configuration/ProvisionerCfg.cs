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
        builder.Property(config => config.Type).HasColumnName("Type").HasMaxLength(20).IsRequired();
        builder.Property(config => config.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
        builder.Property(config => config.ConfigData).HasColumnName("ConfigData").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(config => config.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(config => config.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(config => config.Version).HasColumnName("Version");
        
        builder.HasData(new Provisioner
        {
            Id = 1, Name = "MADProvisioner", Type = "Provisioner", IsEnabled = true, Version = "1.0.0", CreatedAt = new(2025, 6, 18, 0, 0, 0, DateTimeKind.Utc), 
            ModifiedAt = null, ConfigData = "{\"Host\" : \"securix.ch\",\"Port\" : 389,\"UseSsl\" : false, \"BindDn\" : \"Administrator\", \"Password\" : \"Wib12345\", \"BaseDn\" : \"OU=Employees,OU=Users,OU=CH,DC=securix,DC=ch\", \"AuthType\": \"Negotiate\", \"Domain\" : \"SECURIX\"}"
        });
    }
}