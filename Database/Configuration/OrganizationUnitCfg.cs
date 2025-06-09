using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public class OrganizationUnitCfg : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(organizationUnit => organizationUnit.Id);
        builder.Property(organizationUnit => organizationUnit.Id).HasColumnName("id");
        builder.Property(organizationUnit => organizationUnit.CreatedAt).HasColumnName("createdAt");
        builder.Property(organizationUnit => organizationUnit.ModifiedAt).HasColumnName("modifiedAt");
        builder.Property(organizationUnit => organizationUnit.Version).IsRowVersion().HasColumnName("version");
    }
}