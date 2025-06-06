using Core.Domain.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class ResourceCfg : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");
        builder.HasKey(resource => resource.Id);
        builder.Property(resource => resource.ConnectorType).HasColumnName("connectorType");
    }
}