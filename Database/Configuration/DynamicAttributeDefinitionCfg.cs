using Core.Domain.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class DynamicAttributeDefinitionCfg : IEntityTypeConfiguration<DynamicAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<DynamicAttributeDefinition> builder)
    {
        builder.ToTable("attributeDefinitions");
        builder.HasKey(definition => definition.Id);
        builder.Property(definition => definition.DisplayName).HasColumnName("DisplayName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.SystemName).HasColumnName("SystemName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.DataType).HasColumnName("DataType");
        builder.Property(definition => definition.TargetEntity).HasColumnName("TargetEntity");
        builder.Property(definition => definition.MaxLength).HasColumnName("MaxLength");
        builder.Property(definition => definition.IsRequired).HasColumnName("IsRequired");
        builder.Property(definition => definition.Description).HasColumnName("Description");
    }
}