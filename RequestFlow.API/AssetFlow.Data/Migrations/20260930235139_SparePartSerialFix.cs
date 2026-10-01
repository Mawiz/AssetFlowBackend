using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class SparePartSerialFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginPartInventoryBatchId",
                table: "PartSerialNumbers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PartInventoryBatches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginPartInventoryBatchId",
                table: "PartSerialNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PartInventoryBatches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
