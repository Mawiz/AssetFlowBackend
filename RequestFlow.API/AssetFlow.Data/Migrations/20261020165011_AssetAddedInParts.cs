using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetAddedInParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_AssetId",
                table: "Parts",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Assets_AssetId",
                table: "Parts",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Assets_AssetId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_AssetId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Parts");
        }
    }
}
