using Core.Domain.Dynamic;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class DynamicAttributeDefinitionCfg : IEntityTypeConfiguration<DynamicAttributeDefinition>
{
    private static readonly Guid FirstNameId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LastNameId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid EmailId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OrganizationId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public void Configure(EntityTypeBuilder<DynamicAttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");
        builder.HasKey(definition => definition.Id);
        builder.Property(definition => definition.Id).HasColumnName("id");
        builder.Property(definition => definition.DisplayName).HasColumnName("DisplayName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.SystemName).HasColumnName("SystemName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.DataType).HasColumnName("DataType");
        builder.Property(definition => definition.TargetEntity).HasColumnName("TargetEntity");
        builder.Property(definition => definition.MaxLength).HasColumnName("MaxLength");
        builder.Property(definition => definition.IsRequired).HasColumnName("IsRequired");
        builder.Property(definition => definition.Description).HasColumnName("Description");

        builder.HasData(
            new DynamicAttributeDefinition(FirstNameId, "First name", "FIRSTNAME", AttributeDataType.String, MaxLength: 64, IsRequired: true),
            new DynamicAttributeDefinition(LastNameId, "Last name", "LASTNAME", AttributeDataType.String, MaxLength: 64),
            new DynamicAttributeDefinition(EmailId, "Email", "EMAIL", AttributeDataType.String, MaxLength: 256),
            new DynamicAttributeDefinition(AccountId, "Account", "ACCOUNTREF", AttributeDataType.Guid, typeof(Identity)),
            new DynamicAttributeDefinition(OrganizationId, "OrgUnit", "OUREF", AttributeDataType.Guid, typeof(Identity)));
    }
}