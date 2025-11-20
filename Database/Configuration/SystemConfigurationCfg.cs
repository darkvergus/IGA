using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class SystemConfigurationCfg  : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfiguration");
        builder.HasKey(systemConfiguration => systemConfiguration.Id);
        builder.Property(systemConfiguration => systemConfiguration.Id).HasColumnName("Id");
        builder.Property(systemConfiguration => systemConfiguration.Type).HasColumnName("Type");
        builder.Property(systemConfiguration => systemConfiguration.CollectorName).HasColumnName("CollectorName");
        builder.Property(systemConfiguration => systemConfiguration.ProvisionerName).HasColumnName("ProvisionerName");
        builder.Property(systemConfiguration => systemConfiguration.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(systemConfiguration => systemConfiguration.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(systemConfiguration => systemConfiguration.AttrHash).HasColumnName("AttrHash");
        builder.Property(systemConfiguration => systemConfiguration.DataSelection).HasColumnName("DataSelection").HasConversion<string>().HasMaxLength(32);
        builder.Property(systemConfiguration => systemConfiguration.IdentityDataModelXml).HasColumnName("IdentityDataModelXml").HasColumnType("nvarchar(max)");
        builder.Property(systemConfiguration => systemConfiguration.PermissionDataModelXml).HasColumnName("PermissionDataModelXml").HasColumnType("nvarchar(max)");
        builder.Property(systemConfiguration => systemConfiguration.CollectorConnectionConfigurationJson).HasColumnName("CollectorConnectionConfigurationJson").HasColumnType("nvarchar(max)");
        builder.Property(systemConfiguration => systemConfiguration.ProvisionerConnectionConfigurationJson).HasColumnName("ProvisionerConnectionConfigurationJson").HasColumnType("nvarchar(max)");
    }
}