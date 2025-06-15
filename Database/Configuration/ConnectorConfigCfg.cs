using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class ConnectorConfigCfg : IEntityTypeConfiguration<ConnectorConfig>
{
    public void Configure(EntityTypeBuilder<ConnectorConfig> builder)
    {
        builder.ToTable("ConnectorConfigs");
        builder.HasKey(config => config.Id);
        builder.Property(config => config.Id).HasColumnName("Id");
        builder.Property(config => config.ConnectorName).HasColumnName("ConnectorName").HasMaxLength(100).IsRequired();
        builder.Property(config => config.ConnectorType).HasColumnName("ConnectorType").HasMaxLength(20).IsRequired();
        builder.Property(config => config.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
        builder.Property(config => config.ConfigData).HasColumnName("ConfigData").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(config => config.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(config => config.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(config => config.Version).HasColumnName("Version");

        builder.HasData(new ConnectorConfig
        {
            Id = 1, ConnectorName = "CsvCollector", ConnectorType = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new DateTime(2025, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        },
        new ConnectorConfig
        {
            Id = 2, ConnectorName = "LDAPCollector", ConnectorType = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        });
    }
}