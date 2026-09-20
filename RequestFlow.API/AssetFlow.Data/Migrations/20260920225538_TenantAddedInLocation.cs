using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantAddedInLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LocationTypes_Code",
                table: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId_Code",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Locations");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "LocationTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_TenantId",
                table: "LocationTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId",
                table: "Locations",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Tenants_TenantId",
                table: "Locations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationTypes_Tenants_TenantId",
                table: "LocationTypes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Tenants_TenantId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationTypes_Tenants_TenantId",
                table: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_LocationTypes_TenantId",
                table: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_TenantId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LocationTypes");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LocationTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Locations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_Code",
                table: "LocationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId_Code",
                table: "Locations",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
