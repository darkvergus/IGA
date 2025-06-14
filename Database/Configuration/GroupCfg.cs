using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class GroupCfg : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id).HasColumnName("Id");
        builder.Property(group => group.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(group => group.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(group => group.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(group => group.AttrHash).HasColumnName("AttrHash");
    }
}