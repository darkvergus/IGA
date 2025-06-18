using Domain.Core.Entities.Connector;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class ConnectorCfg : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.ToTable("Connector");
        builder.HasKey(config => config.Id);
        builder.Property(config => config.Id).HasColumnName("Id");
        builder.Property(config => config.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(config => config.Type).HasColumnName("Type").HasMaxLength(20).IsRequired();
        builder.Property(config => config.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
        builder.Property(config => config.ConfigData).HasColumnName("ConfigData").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(config => config.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(config => config.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(config => config.Version).HasColumnName("Version");

        builder.HasData(new Connector
        {
            Id = 1, Name = "CsvCollector", Type = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new(2025, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        },
        new Connector
        {
            Id = 2, Name = "LDAPCollector", Type = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        });
    }
}