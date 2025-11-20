using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    TargetEntity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    keyType = table.Column<int>(type: "int", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Collector",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConfigData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collector", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BusinessKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrganizationUnit = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Manager = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provisioner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConfigData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provisioner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvisionerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataSelection = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IdentityDataModelXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermissionDataModelXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectorConnectionConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisionerConnectionConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AttrHash = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfiguration", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AttributeDefinitions",
                columns: new[] { "Id", "DataType", "Description", "DisplayName", "IsRequired", "keyType", "MaxLength", "SystemName", "TargetEntity" },
                values: new object[,]
                {
                    { new Guid("0e842d9d-d341-4594-a119-78e0f9fc4ab3"), 9, null, "Manager", false, 0, null, "MANAGER", "Core.Entities.Identity, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { new Guid("2e1b4696-a1ae-48df-a8cc-99de19dda5a6"), 0, null, "Email", false, 0, 256, "EMAIL", null },
                    { new Guid("756d52d7-c5ef-4e71-baba-8a8014509a73"), 0, null, "Last name", false, 0, 64, "LASTNAME", null },
                    { new Guid("7e8a804f-4945-4ce8-98a2-3c2560d45748"), 0, null, "Identity Id", true, 0, 64, "IDENTITYID", null },
                    { new Guid("921c1e4c-ff5c-47df-a5f5-e8218cbed540"), 9, null, "OrgUnit", false, 0, null, "OUREF", "Core.Entities.OrganizationUnit, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { new Guid("a56161ba-6f0f-4c35-bf93-93f94e69eca1"), 0, null, "Business Key", true, 0, 64, "BUSINESSKEY", null },
                    { new Guid("a70978ed-b2c3-4322-99d4-71e3a01d9d77"), 0, null, "Name", false, 0, 64, "NAME", null },
                    { new Guid("bdb5ca69-d0c6-4cb0-aaef-9999a3222b0c"), 4, null, "Valid To", false, 0, null, "VALIDTO", null },
                    { new Guid("cd38e4af-66e4-48e4-8c8d-fc075b789e1a"), 4, null, "Valid From", false, 0, null, "VALIDFROM", null },
                    { new Guid("d2ebb9c6-14d5-4927-b80e-88c06533c504"), 0, null, "First name", true, 0, 64, "FIRSTNAME", null },
                    { new Guid("ef9f5d79-c514-44ef-8f16-7bff193f7a47"), 9, null, "Identity", false, 0, null, "IDENTITYREF", "Core.Entities.Identity, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" }
                });

            migrationBuilder.InsertData(
                table: "Collector",
                columns: new[] { "Id", "ConfigData", "CreatedAt", "IsEnabled", "ModifiedAt", "Name", "Type", "Version" },
                values: new object[,]
                {
                    { 1, "{}", new DateTime(2025, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "CsvCollector", "Collector", "1.0.0" },
                    { 2, "{}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "MADCollector", "Collector", "1.0.0" }
                });

            migrationBuilder.InsertData(
                table: "Provisioner",
                columns: new[] { "Id", "ConfigData", "CreatedAt", "IsEnabled", "ModifiedAt", "Name", "Type", "Version" },
                values: new object[] { 1, "{\"Host\" : \"securix.ch\",\"Port\" : 389,\"UseSsl\" : false, \"BindDn\" : \"Administrator\", \"Password\" : \"Wib12345\", \"BaseDn\" : \"OU=Employees,OU=Users,OU=CH,DC=securix,DC=ch\", \"AuthType\": \"Negotiate\", \"Domain\" : \"SECURIX\"}", new DateTime(2025, 6, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "MADProvisioner", "Provisioner", "1.0.0" });

            migrationBuilder.CreateIndex(
                name: "IX_Identities_BusinessKey",
                table: "Identities",
                column: "BusinessKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status_Type_InstanceId",
                table: "Jobs",
                columns: new[] { "Status", "Type", "InstanceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropTable(
                name: "Collector");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Identities");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Provisioner");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "SystemConfiguration");
        }
    }
}
