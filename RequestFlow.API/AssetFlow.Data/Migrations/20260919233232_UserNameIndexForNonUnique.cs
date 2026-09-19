using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserNameIndexForNonUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_TenantId",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_UserName",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_TenantId",
                table: "ApplicationRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "ApplicationRoles");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_TenantId_NormalizedEmail",
                table: "ApplicationUsers",
                columns: new[] { "TenantId", "NormalizedEmail" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_TenantId_NormalizedUserName",
                table: "ApplicationUsers",
                columns: new[] { "TenantId", "NormalizedUserName" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_TenantId_NormalizedName",
                table: "ApplicationRoles",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "ApplicationRoles",
                column: "NormalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_TenantId_NormalizedEmail",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_TenantId_NormalizedUserName",
                table: "ApplicationUsers");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_TenantId_NormalizedName",
                table: "ApplicationRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "ApplicationRoles");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_TenantId",
                table: "ApplicationUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UserName",
                table: "ApplicationUsers",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_TenantId",
                table: "ApplicationRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "ApplicationRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }
    }
}
