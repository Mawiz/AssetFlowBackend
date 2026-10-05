using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartReplacementAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartReplacementId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    OldAssetComponentId = table.Column<int>(type: "int", nullable: true),
                    OldPartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldPartSerialNumberId = table.Column<int>(type: "int", nullable: true),
                    NewPartId = table.Column<int>(type: "int", nullable: false),
                    NewPartSerialNumberId = table.Column<int>(type: "int", nullable: true),
                    NewPartInventoryBatchId = table.Column<int>(type: "int", nullable: true),
                    NewAssetComponentId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstalledByUserId = table.Column<int>(type: "int", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "int", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovalReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    InstallationLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PartTransactionId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartReplacements_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_ApplicationUsers_InstalledByUserId",
                        column: x => x.InstalledByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartReplacements_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_ApplicationUsers_RemovedByUserId",
                        column: x => x.RemovedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_AssetComponents_NewAssetComponentId",
                        column: x => x.NewAssetComponentId,
                        principalTable: "AssetComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_AssetComponents_OldAssetComponentId",
                        column: x => x.OldAssetComponentId,
                        principalTable: "AssetComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_PartInventoryBatches_NewPartInventoryBatchId",
                        column: x => x.NewPartInventoryBatchId,
                        principalTable: "PartInventoryBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_PartSerialNumbers_NewPartSerialNumberId",
                        column: x => x.NewPartSerialNumberId,
                        principalTable: "PartSerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_PartSerialNumbers_OldPartSerialNumberId",
                        column: x => x.OldPartSerialNumberId,
                        principalTable: "PartSerialNumbers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_PartTransactions_PartTransactionId",
                        column: x => x.PartTransactionId,
                        principalTable: "PartTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_Parts_NewPartId",
                        column: x => x.NewPartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartReplacements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartReplacements_WorkOrders_WorkOrderId",
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
                    { 123, null, "PartReplacement" },
                    { 151, 123, "PartReplacement.View" },
                    { 152, 123, "PartReplacement.Create" },
                    { 153, 123, "PartReplacement.Validate" },
                    { 154, 123, "PartReplacement.Replace" },
                    { 155, 123, "PartReplacement.Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_AssetId",
                table: "PartReplacements",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_CreatedById",
                table: "PartReplacements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_FromLocationId",
                table: "PartReplacements",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_InstalledByUserId",
                table: "PartReplacements",
                column: "InstalledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_ModifiedById",
                table: "PartReplacements",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_NewAssetComponentId",
                table: "PartReplacements",
                column: "NewAssetComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_NewPartId",
                table: "PartReplacements",
                column: "NewPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_NewPartInventoryBatchId",
                table: "PartReplacements",
                column: "NewPartInventoryBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_NewPartSerialNumberId",
                table: "PartReplacements",
                column: "NewPartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_OldAssetComponentId",
                table: "PartReplacements",
                column: "OldAssetComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_OldPartSerialNumberId",
                table: "PartReplacements",
                column: "OldPartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_PartTransactionId",
                table: "PartReplacements",
                column: "PartTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_RemovedByUserId",
                table: "PartReplacements",
                column: "RemovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_TenantId",
                table: "PartReplacements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PartReplacements_WorkOrderId",
                table: "PartReplacements",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartReplacements");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 151);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 152);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 153);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 154);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 155);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "PartReplacementId",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "PartTransactions");
        }
    }
}
