using Domain.Core.Entities.Collector;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class CollectorCfg : IEntityTypeConfiguration<Collector>
{
    public void Configure(EntityTypeBuilder<Collector> builder)
    {
        builder.ToTable("Collector");
        builder.HasKey(config => config.Id);
        builder.Property(config => config.Id).HasColumnName("Id");
        builder.Property(config => config.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(config => config.Type).HasColumnName("Type").HasMaxLength(20).IsRequired();
        builder.Property(config => config.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
        builder.Property(config => config.ConfigData).HasColumnName("ConfigData").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(config => config.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(config => config.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(config => config.Version).HasColumnName("Version");

        builder.HasData(new Collector
        {
            Id = 1, Name = "CsvCollector", Type = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new(2025, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        },
        new Collector
        {
            Id = 2, Name = "MADCollector", Type = "Collector", IsEnabled = true, ConfigData = "{}",
            CreatedAt = new(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), ModifiedAt = null, Version = "1.0.0"
        });
    }
}