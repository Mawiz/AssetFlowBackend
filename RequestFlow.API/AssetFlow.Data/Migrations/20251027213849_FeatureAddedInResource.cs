using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeatureAddedInResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FeatureId",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_FeatureId",
                table: "Resources",
                column: "FeatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Resources_FeatureId",
                table: "Resources",
                column: "FeatureId",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Resources_FeatureId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_FeatureId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "Resources");
        }
    }
}
