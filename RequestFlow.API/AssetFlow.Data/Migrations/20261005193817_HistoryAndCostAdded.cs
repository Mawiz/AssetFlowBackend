using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class HistoryAndCostAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderLaborRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderLaborRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderLaborRecords_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderLaborRecords_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderLaborRecords_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderLaborRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderLaborRecords_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceCostRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    CostType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CostDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartReplacementId = table.Column<int>(type: "int", nullable: true),
                    PartId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LaborRecordId = table.Column<int>(type: "int", nullable: true),
                    ExternalServiceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceCostRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_PartReplacements_PartReplacementId",
                        column: x => x.PartReplacementId,
                        principalTable: "PartReplacements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_WorkOrderLaborRecords_LaborRecordId",
                        column: x => x.LaborRecordId,
                        principalTable: "WorkOrderLaborRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceCostRecords_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 124, null, "AssetHistory" },
                    { 125, null, "CostManagement" },
                    { 156, 124, "AssetHistory.View" },
                    { 157, 125, "CostManagement.View" },
                    { 158, 125, "CostManagement.Create" },
                    { 159, 125, "CostManagement.Update" },
                    { 160, 125, "CostManagement.Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_AssetId",
                table: "MaintenanceCostRecords",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_CostDate",
                table: "MaintenanceCostRecords",
                column: "CostDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_CreatedById",
                table: "MaintenanceCostRecords",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_CreatedByUserId",
                table: "MaintenanceCostRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_LaborRecordId",
                table: "MaintenanceCostRecords",
                column: "LaborRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_ModifiedById",
                table: "MaintenanceCostRecords",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_PartId",
                table: "MaintenanceCostRecords",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_PartReplacementId",
                table: "MaintenanceCostRecords",
                column: "PartReplacementId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_TenantId",
                table: "MaintenanceCostRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCostRecords_WorkOrderId",
                table: "MaintenanceCostRecords",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLaborRecords_CreatedById",
                table: "WorkOrderLaborRecords",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLaborRecords_ModifiedById",
                table: "WorkOrderLaborRecords",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLaborRecords_TenantId",
                table: "WorkOrderLaborRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLaborRecords_UserId",
                table: "WorkOrderLaborRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLaborRecords_WorkOrderId",
                table: "WorkOrderLaborRecords",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceCostRecords");

            migrationBuilder.DropTable(
                name: "WorkOrderLaborRecords");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 156);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 157);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 125);
        }
    }
}
