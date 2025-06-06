using Core.Domain.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class IdentityCfg : IEntityTypeConfiguration<Identity>
{
    public void Configure(EntityTypeBuilder<Identity> builder)
    {
        builder.ToTable("identities");
        builder.HasKey(identity => identity.Id);
        builder.Property(identity => identity.Id).HasColumnName("id");
        builder.Property(identity => identity.CreatedAt).HasColumnName("createdAt");
        builder.Property(identity => identity.ModifiedAt).HasColumnName("modifiedAt");
        builder.Property(identity => identity.Version).IsRowVersion().HasColumnName("version");
    }
}