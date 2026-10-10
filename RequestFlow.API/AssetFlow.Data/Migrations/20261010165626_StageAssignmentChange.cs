using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class StageAssignmentChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssigneeTypeId",
                schema: "Workflow",
                table: "ModuleStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentValue",
                schema: "Workflow",
                table: "ModuleStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInitialStage",
                schema: "Workflow",
                table: "ModuleStages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssigneeTypeId",
                schema: "Workflow",
                table: "ModuleStages");

            migrationBuilder.DropColumn(
                name: "AssignmentValue",
                schema: "Workflow",
                table: "ModuleStages");

            migrationBuilder.DropColumn(
                name: "IsInitialStage",
                schema: "Workflow",
                table: "ModuleStages");
        }
    }
}
