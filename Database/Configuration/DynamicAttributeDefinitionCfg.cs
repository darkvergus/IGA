using Core.Domain.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class DynamicAttributeDefinitionCfg : IEntityTypeConfiguration<DynamicAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<DynamicAttributeDefinition> builder)
    {
        builder.ToTable("attributeDefinitions");
        builder.HasKey(definition => definition.Id);
        builder.Property(definition => definition.Name).HasColumnName("name").HasMaxLength(64);
        builder.Property(definition => definition.TargetEntity).HasColumnName("targetEntity").HasMaxLength(64);
        builder.Property(definition => definition.DataType).HasColumnName("dataType");
        builder.Property(definition => definition.Description).HasColumnName("description");
    }
}