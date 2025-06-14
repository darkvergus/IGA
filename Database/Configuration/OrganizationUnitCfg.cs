using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class OrganizationUnitCfg : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(organizationUnit => organizationUnit.Id);
        builder.Property(organizationUnit => organizationUnit.Id).HasColumnName("Id");
        builder.Property(organizationUnit => organizationUnit.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(organizationUnit => organizationUnit.ModifiedAt).HasColumnName("ModifiedAt");
        builder.Property(organizationUnit => organizationUnit.Version).HasColumnName("Version").IsConcurrencyToken();
        builder.Property(organizationUnit => organizationUnit.AttrHash).HasColumnName("AttrHash");
    }
}