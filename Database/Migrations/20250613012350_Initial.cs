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
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    modifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    modifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrganizationUnit = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    modifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Manager = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    modifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    modifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "AttributeDefinitions",
                columns: new[] { "id", "DataType", "Description", "DisplayName", "IsRequired", "keyType", "MaxLength", "SystemName", "TargetEntity" },
                values: new object[,]
                {
                    { new Guid("0e842d9d-d341-4594-a119-78e0f9fc4ab3"), 9, null, "Manager", false, 0, null, "MANAGER", "Core.Domain.Entities.Identity, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { new Guid("2e1b4696-a1ae-48df-a8cc-99de19dda5a6"), 0, null, "Email", false, 0, 256, "EMAIL", null },
                    { new Guid("756d52d7-c5ef-4e71-baba-8a8014509a73"), 0, null, "Last name", false, 0, 64, "LASTNAME", null },
                    { new Guid("921c1e4c-ff5c-47df-a5f5-e8218cbed540"), 9, null, "OrgUnit", false, 0, null, "OUREF", "Core.Domain.Entities.OrganizationUnit, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { new Guid("d2ebb9c6-14d5-4927-b80e-88c06533c504"), 0, null, "First name", true, 0, 64, "FIRSTNAME", null },
                    { new Guid("ef9f5d79-c514-44ef-8f16-7bff193f7a47"), 9, null, "Identity", false, 0, null, "IDENTITYREF", "Core.Domain.Entities.Identity, Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Identities");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
