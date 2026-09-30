using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartChangesInTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartTransactionId",
                table: "PartInventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_PartTransactionId",
                table: "PartInventories",
                column: "PartTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventories_PartTransactions_PartTransactionId",
                table: "PartInventories",
                column: "PartTransactionId",
                principalTable: "PartTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInventories_PartTransactions_PartTransactionId",
                table: "PartInventories");

            migrationBuilder.DropIndex(
                name: "IX_PartInventories_PartTransactionId",
                table: "PartInventories");

            migrationBuilder.DropColumn(
                name: "PartTransactionId",
                table: "PartInventories");
        }
    }
}
