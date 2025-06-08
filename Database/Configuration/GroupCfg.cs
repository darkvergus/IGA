using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class GroupCfg : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
        builder.HasKey(group => group.Id);
        builder.Property(group => group.CreatedAt).HasColumnName("createdAt");
        builder.Property(group => group.ModifiedAt).HasColumnName("modifiedAt");
        builder.Property(group => group.Version).IsRowVersion().HasColumnName("version");
    }
}