using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class SparePartFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInventories_PartSerialNumbers_PartSerialNumberId",
                table: "PartInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PartInventories_PartTransactions_PartTransactionId",
                table: "PartInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTransactions_PartSerialNumbers_PartSerialNumberId",
                table: "PartTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PartInventories_PartSerialNumberId",
                table: "PartInventories");

            migrationBuilder.DropIndex(
                name: "IX_PartInventories_PartTransactionId",
                table: "PartInventories");

            migrationBuilder.DropIndex(
                name: "IX_PartInventories_TenantId_PartId_LocationId_PartSerialNumberId",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "PartSerialNumberId",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "PartTransactionId",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartInventories");

            migrationBuilder.RenameColumn(
                name: "PartSerialNumberId",
                table: "PartTransactions",
                newName: "ReturnedFromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PartTransactions_PartSerialNumberId",
                table: "PartTransactions",
                newName: "IX_PartTransactions_ReturnedFromUserId");

            migrationBuilder.RenameColumn(
                name: "QuantityReserved",
                table: "PartInventories",
                newName: "TotalQuantity");

            migrationBuilder.RenameColumn(
                name: "QuantityAvailable",
                table: "PartInventories",
                newName: "QuarantineQuantity");

            migrationBuilder.AddColumn<int>(
                name: "IssuedToUserId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartInventoryBatchId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartInventoryId",
                table: "PartTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PartTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExpectedLifeUnit",
                table: "PartSerialNumbers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedLifeValue",
                table: "PartSerialNumbers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "PartSerialNumbers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartInventoryBatchId",
                table: "PartSerialNumbers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableQuantity",
                table: "PartInventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FaultyQuantity",
                table: "PartInventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedQuantity",
                table: "PartInventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PartInventoryBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    PartInventoryId = table.Column<int>(type: "int", nullable: false),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    BatchReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedLifeValue = table.Column<int>(type: "int", nullable: true),
                    ExpectedLifeUnit = table.Column<int>(type: "int", nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FaultyQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuarantineQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInventoryBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_PartInventories_PartInventoryId",
                        column: x => x.PartInventoryId,
                        principalTable: "PartInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventoryBatches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartTransactionSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartTransactionId = table.Column<int>(type: "int", nullable: false),
                    PartSerialNumberId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartTransactionSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartTransactionSerials_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartTransactionSerials_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartTransactionSerials_PartSerialNumbers_PartSerialNumberId",
                        column: x => x.PartSerialNumberId,
                        principalTable: "PartSerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTransactionSerials_PartTransactions_PartTransactionId",
                        column: x => x.PartTransactionId,
                        principalTable: "PartTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_IssuedToUserId",
                table: "PartTransactions",
                column: "IssuedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_PartInventoryBatchId",
                table: "PartTransactions",
                column: "PartInventoryBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactions_PartInventoryId",
                table: "PartTransactions",
                column: "PartInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSerialNumbers_PartInventoryBatchId",
                table: "PartSerialNumbers",
                column: "PartInventoryBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_TenantId_PartId_LocationId",
                table: "PartInventories",
                columns: new[] { "TenantId", "PartId", "LocationId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_CreatedById",
                table: "PartInventoryBatches",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_ModifiedById",
                table: "PartInventoryBatches",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_PartId",
                table: "PartInventoryBatches",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_PartInventoryId",
                table: "PartInventoryBatches",
                column: "PartInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_SupplierId",
                table: "PartInventoryBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventoryBatches_TenantId_BatchReference",
                table: "PartInventoryBatches",
                columns: new[] { "TenantId", "BatchReference" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactionSerials_CreatedById",
                table: "PartTransactionSerials",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactionSerials_ModifiedById",
                table: "PartTransactionSerials",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactionSerials_PartSerialNumberId",
                table: "PartTransactionSerials",
                column: "PartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTransactionSerials_PartTransactionId",
                table: "PartTransactionSerials",
                column: "PartTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartSerialNumbers_PartInventoryBatches_PartInventoryBatchId",
                table: "PartSerialNumbers",
                column: "PartInventoryBatchId",
                principalTable: "PartInventoryBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTransactions_ApplicationUsers_IssuedToUserId",
                table: "PartTransactions",
                column: "IssuedToUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTransactions_ApplicationUsers_ReturnedFromUserId",
                table: "PartTransactions",
                column: "ReturnedFromUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTransactions_PartInventories_PartInventoryId",
                table: "PartTransactions",
                column: "PartInventoryId",
                principalTable: "PartInventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTransactions_PartInventoryBatches_PartInventoryBatchId",
                table: "PartTransactions",
                column: "PartInventoryBatchId",
                principalTable: "PartInventoryBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartSerialNumbers_PartInventoryBatches_PartInventoryBatchId",
                table: "PartSerialNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTransactions_ApplicationUsers_IssuedToUserId",
                table: "PartTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTransactions_ApplicationUsers_ReturnedFromUserId",
                table: "PartTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTransactions_PartInventories_PartInventoryId",
                table: "PartTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTransactions_PartInventoryBatches_PartInventoryBatchId",
                table: "PartTransactions");

            migrationBuilder.DropTable(
                name: "PartInventoryBatches");

            migrationBuilder.DropTable(
                name: "PartTransactionSerials");

            migrationBuilder.DropIndex(
                name: "IX_PartTransactions_IssuedToUserId",
                table: "PartTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PartTransactions_PartInventoryBatchId",
                table: "PartTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PartTransactions_PartInventoryId",
                table: "PartTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PartSerialNumbers_PartInventoryBatchId",
                table: "PartSerialNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PartInventories_TenantId_PartId_LocationId",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "IssuedToUserId",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "PartInventoryBatchId",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "PartInventoryId",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PartTransactions");

            migrationBuilder.DropColumn(
                name: "ExpectedLifeUnit",
                table: "PartSerialNumbers");

            migrationBuilder.DropColumn(
                name: "ExpectedLifeValue",
                table: "PartSerialNumbers");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PartSerialNumbers");

            migrationBuilder.DropColumn(
                name: "PartInventoryBatchId",
                table: "PartSerialNumbers");

            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "FaultyQuantity",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                table: "PartInventories");

            migrationBuilder.RenameColumn(
                name: "ReturnedFromUserId",
                table: "PartTransactions",
                newName: "PartSerialNumberId");

            migrationBuilder.RenameIndex(
                name: "IX_PartTransactions_ReturnedFromUserId",
                table: "PartTransactions",
                newName: "IX_PartTransactions_PartSerialNumberId");

            migrationBuilder.RenameColumn(
                name: "TotalQuantity",
                table: "PartInventories",
                newName: "QuantityReserved");

            migrationBuilder.RenameColumn(
                name: "QuarantineQuantity",
                table: "PartInventories",
                newName: "QuantityAvailable");

            migrationBuilder.AddColumn<int>(
                name: "PartSerialNumberId",
                table: "PartInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartTransactionId",
                table: "PartInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PartInventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_PartSerialNumberId",
                table: "PartInventories",
                column: "PartSerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_PartTransactionId",
                table: "PartInventories",
                column: "PartTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_TenantId_PartId_LocationId_PartSerialNumberId",
                table: "PartInventories",
                columns: new[] { "TenantId", "PartId", "LocationId", "PartSerialNumberId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [PartSerialNumberId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventories_PartSerialNumbers_PartSerialNumberId",
                table: "PartInventories",
                column: "PartSerialNumberId",
                principalTable: "PartSerialNumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventories_PartTransactions_PartTransactionId",
                table: "PartInventories",
                column: "PartTransactionId",
                principalTable: "PartTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTransactions_PartSerialNumbers_PartSerialNumberId",
                table: "PartTransactions",
                column: "PartSerialNumberId",
                principalTable: "PartSerialNumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
