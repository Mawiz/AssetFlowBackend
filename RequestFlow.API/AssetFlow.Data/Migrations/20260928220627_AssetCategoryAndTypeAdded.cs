using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetCategoryAndTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCategories_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetCategories_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    AssetCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTypes_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetTypes_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetTypes_AssetCategories_AssetCategoryId",
                        column: x => x.AssetCategoryId,
                        principalTable: "AssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 48, null, "AssetCategory" },
                    { 49, null, "AssetType" },
                    { 50, 48, "AssetCategory.View" },
                    { 51, 48, "AssetCategory.Create" },
                    { 52, 48, "AssetCategory.Update" },
                    { 53, 48, "AssetCategory.Delete" },
                    { 54, 49, "AssetType.View" },
                    { 55, 49, "AssetType.Create" },
                    { 56, 49, "AssetType.Update" },
                    { 57, 49, "AssetType.Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_CreatedById",
                table: "AssetCategories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_ModifiedById",
                table: "AssetCategories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_TenantId_Code",
                table: "AssetCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypes_AssetCategoryId",
                table: "AssetTypes",
                column: "AssetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypes_CreatedById",
                table: "AssetTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypes_ModifiedById",
                table: "AssetTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypes_TenantId_AssetCategoryId_Code",
                table: "AssetTypes",
                columns: new[] { "TenantId", "AssetCategoryId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetTypes");

            migrationBuilder.DropTable(
                name: "AssetCategories");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 49);
        }
    }
}
