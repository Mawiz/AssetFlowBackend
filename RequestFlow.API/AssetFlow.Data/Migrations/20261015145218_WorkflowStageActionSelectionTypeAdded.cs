using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowStageActionSelectionTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectionTypeId",
                schema: "Workflow",
                table: "StagesActions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectionTypeId",
                schema: "Workflow",
                table: "StagesActions");
        }
    }
}
