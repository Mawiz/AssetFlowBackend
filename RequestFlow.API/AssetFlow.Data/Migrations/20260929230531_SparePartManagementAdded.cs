using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class SparePartManagementAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartCategories",
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
                    table.PrimaryKey("PK_PartCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartCategories_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartCategories_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    PartNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PartName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartCategoryId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpectedLifeValue = table.Column<int>(type: "int", nullable: true),
                    ExpectedLifeUnit = table.Column<int>(type: "int", nullable: true),
                    MinStockLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxStockLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsSerialized = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Parts_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Parts_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartSerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    WarrantyStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartSerialNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartSerialNumbers_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartSerialNumbers_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartSerialNumbers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartSerialNumbers_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartSerialNumbers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    PartSerialNumberId = table.Column<int>(type: "int", nullable: true),
                    QuantityAvailable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartInventories_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartInventories_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartInventories_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventories_PartSerialNumbers_PartSerialNumberId",
                        column: x => x.PartSerialNumberId,
                        principalTable: "PartSerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventories_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    PartSerialNumberId = table.Column<int>(type: "int", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartTransactions_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartTransactions_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartTransactions_ApplicationUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactions_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactions_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactions_PartSerialNumbers_PartSerialNumberId",
                        column: x => x.PartSerialNumberId,
                        principalTable: "PartSerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactions_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 68, null, "PartCategory" },
                    { 69, null, "Part" },
                    { 70, null, "PartInventory" },
                    { 71, null, "PartTransaction" },
                    { 72, 68, "PartCategory.View" },
                    { 73, 68, "PartCategory.Create" },
                    { 74, 68, "PartCategory.Update" },
                    { 75, 68, "PartCategory.Delete" },
                    { 76, 69, "Part.View" },
                    { 77, 69, "Part.Create" },
                    { 78, 69, "Part.Update" },
                    { 79, 69, "Part.Delete" },
                    { 80, 70, "PartInventory.View" },
                    { 81, 70, "PartInventory.Create" },
                    { 82, 70, "PartInventory.Update" },
                    { 83, 70, "PartInventory.Delete" },
                    { 84, 71, "PartTransaction.View" },
                    { 85, 71, "PartTransaction.Create" },
                    { 86, 71, "PartTransaction.Update" },
                    { 87, 71, "PartTransaction.Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_CreatedById",
                table: "PartCategories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_ModifiedById",
                table: "PartCategories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_TenantId_Code",
                table: "PartCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_CreatedById",
                table: "PartInventories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_LocationId",
                table: "PartInventories",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_ModifiedById",
                table: "PartInventories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_PartId",
                table: "PartInventories",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_PartSerialNumberId",
                table: "PartInventories",
                column: "PartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_TenantId_PartId_LocationId_PartSerialNumberId",
                table: "PartInventories",
                columns: new[] { "TenantId", "PartId", "LocationId", "PartSerialNumberId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [PartSerialNumberId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CreatedById",
                table: "Parts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ModifiedById",
                table: "Parts",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCategoryId",
                table: "Parts",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_TenantId_PartNumber",
                table: "Parts",
                columns: new[] { "TenantId", "PartNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_CreatedById",
                table: "PartSerialNumbers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_LocationId",
                table: "PartSerialNumbers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_ModifiedById",
                table: "PartSerialNumbers",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_PartId",
                table: "PartSerialNumbers",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_TenantId_SerialNumber",
                table: "PartSerialNumbers",
                columns: new[] { "TenantId", "SerialNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_CreatedById",
                table: "PartTransactions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_FromLocationId",
                table: "PartTransactions",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_ModifiedById",
                table: "PartTransactions",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_PartId",
                table: "PartTransactions",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_PartSerialNumberId",
                table: "PartTransactions",
                column: "PartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_PerformedByUserId",
                table: "PartTransactions",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_TenantId",
                table: "PartTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_ToLocationId",
                table: "PartTransactions",
                column: "ToLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartInventories");

            migrationBuilder.DropTable(
                name: "PartTransactions");

            migrationBuilder.DropTable(
                name: "PartSerialNumbers");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "PartCategories");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 71);
        }
    }
}
