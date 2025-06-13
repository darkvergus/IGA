using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class ResourceCfg : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");
        builder.HasKey(resource => resource.Id);
        builder.Property(resource => resource.Id).HasColumnName("Id");
        builder.Property(resource => resource.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(resource => resource.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(resource => resource.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(resource => resource.AttrHash).HasColumnName("AttrHash");
    }
}