using Core.Dynamic;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configuration;

public sealed class DynamicAttributeDefinitionCfg : IEntityTypeConfiguration<DynamicAttributeDefinition>
{
    private static readonly Guid IdentityId = Guid.Parse("7e8a804f-4945-4ce8-98a2-3c2560d45748");
    private static readonly Guid BusinessKeyId = Guid.Parse("a56161ba-6f0f-4c35-bf93-93f94e69eca1");
    private static readonly Guid FirstNameId = Guid.Parse("d2ebb9c6-14d5-4927-b80e-88c06533c504");
    private static readonly Guid LastNameId = Guid.Parse("756d52d7-c5ef-4e71-baba-8a8014509a73");
    private static readonly Guid EmailId = Guid.Parse("2e1b4696-a1ae-48df-a8cc-99de19dda5a6");
    private static readonly Guid IdentityRefId = Guid.Parse("ef9f5d79-c514-44ef-8f16-7bff193f7a47");
    private static readonly Guid OuRefId = Guid.Parse("921c1e4c-ff5c-47df-a5f5-e8218cbed540");
    private static readonly Guid ManagerId = Guid.Parse("0e842d9d-d341-4594-a119-78e0f9fc4ab3");

    public void Configure(EntityTypeBuilder<DynamicAttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");
        builder.HasKey(definition => definition.Id);
        builder.Property(definition => definition.Id).HasColumnName("Id");
        builder.Property(definition => definition.DisplayName).HasColumnName("DisplayName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.SystemName).HasColumnName("SystemName").IsRequired().HasMaxLength(64);
        builder.Property(definition => definition.DataType).HasColumnName("DataType");
        builder.Property(definition => definition.KeyType).HasColumnName("keyType");
        builder.Property(definition => definition.MaxLength).HasColumnName("MaxLength");
        builder.Property(definition => definition.IsRequired).HasColumnName("IsRequired");
        builder.Property(definition => definition.Description).HasColumnName("Description");
        builder.Property(definition => definition.TargetEntity)
            .HasConversion(type => type == null ? null : type.AssemblyQualifiedName, typeName 
                => typeName == null ? null : Type.GetType(typeName));

        builder.HasData(
            new DynamicAttributeDefinition(BusinessKeyId, "Business Key", "BUSINESSKEY", AttributeDataType.String, MaxLength: 64, IsRequired: true),
            new DynamicAttributeDefinition(IdentityId, "Identity Id", "IDENTITYID", AttributeDataType.String, MaxLength: 64, IsRequired: true),
            new DynamicAttributeDefinition(FirstNameId, "First name", "FIRSTNAME", AttributeDataType.String, MaxLength: 64, IsRequired: true),
            new DynamicAttributeDefinition(LastNameId, "Last name", "LASTNAME", AttributeDataType.String, MaxLength: 64),
            new DynamicAttributeDefinition(EmailId, "Email", "EMAIL", AttributeDataType.String, MaxLength: 256),
            new DynamicAttributeDefinition(IdentityRefId, "Identity", "IDENTITYREF", AttributeDataType.Reference, typeof(Identity)),
            new DynamicAttributeDefinition(OuRefId, "OrgUnit", "OUREF", AttributeDataType.Reference, typeof(OrganizationUnit)),
            new DynamicAttributeDefinition(ManagerId, "Manager", "MANAGER", AttributeDataType.Reference, typeof(Identity)));
    }
}