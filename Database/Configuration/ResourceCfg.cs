using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class ResourceCfg : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");
        builder.HasKey(resource => resource.Id);
        builder.Property(resource => resource.CreatedAt).HasColumnName("createdAt");
        builder.Property(resource => resource.ModifiedAt).HasColumnName("modifiedAt");
        builder.Property(resource => resource.Version).IsRowVersion().HasColumnName("version");
    }
}