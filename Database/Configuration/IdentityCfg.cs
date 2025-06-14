using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class IdentityCfg : IEntityTypeConfiguration<Identity>
{
    public void Configure(EntityTypeBuilder<Identity> builder)
    {
        builder.ToTable("Identities");
        builder.Property(identity => identity.BusinessKey).HasColumnName("BusinessKey").IsRequired();
        builder.HasIndex(identity => identity.BusinessKey).IsUnique();
        builder.HasKey(identity => identity.Id);
        builder.Property(identity => identity.Id).HasColumnName("Id");
        builder.Property(identity => identity.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(identity => identity.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(identity => identity.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(identity => identity.AttrHash).HasColumnName("AttrHash");
    }
}