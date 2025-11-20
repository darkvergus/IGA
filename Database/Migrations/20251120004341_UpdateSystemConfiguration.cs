using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollectorConnectionConfigurationJson",
                table: "SystemConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataSelection",
                table: "SystemConfiguration",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityDataModelXml",
                table: "SystemConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionDataModelXml",
                table: "SystemConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisionerConnectionConfigurationJson",
                table: "SystemConfiguration",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectorConnectionConfigurationJson",
                table: "SystemConfiguration");

            migrationBuilder.DropColumn(
                name: "DataSelection",
                table: "SystemConfiguration");

            migrationBuilder.DropColumn(
                name: "IdentityDataModelXml",
                table: "SystemConfiguration");

            migrationBuilder.DropColumn(
                name: "PermissionDataModelXml",
                table: "SystemConfiguration");

            migrationBuilder.DropColumn(
                name: "ProvisionerConnectionConfigurationJson",
                table: "SystemConfiguration");
        }
    }
}
