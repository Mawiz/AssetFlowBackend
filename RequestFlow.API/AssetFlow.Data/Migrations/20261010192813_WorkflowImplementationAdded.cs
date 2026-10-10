using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowImplementationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleItemMappings",
                schema: "Workflow",
                columns: table => new
                {
                    ModuleItemMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    ReferenceItemType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleItemMappings", x => x.ModuleItemMappingId);
                    table.ForeignKey(
                        name: "FK_ModuleItemMappings_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "Workflow",
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowMasters",
                schema: "Workflow",
                columns: table => new
                {
                    WorkflowMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    ReferenceItemId = table.Column<int>(type: "int", nullable: false),
                    ReferenceItemType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmitBy = table.Column<int>(type: "int", nullable: false),
                    CurrentStageId = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowMasters", x => x.WorkflowMasterId);
                    table.ForeignKey(
                        name: "FK_WorkflowMasters_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "Workflow",
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                schema: "Workflow",
                columns: table => new
                {
                    WorkflowTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleStageId = table.Column<int>(type: "int", nullable: false),
                    StageActionId = table.Column<int>(type: "int", nullable: true),
                    ActionId = table.Column<int>(type: "int", nullable: true),
                    AssigneeTypeId = table.Column<int>(type: "int", nullable: true),
                    AssignmentValue = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedByRoleId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueInHours = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.WorkflowTaskId);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_ModuleStages_ModuleStageId",
                        column: x => x.ModuleStageId,
                        principalSchema: "Workflow",
                        principalTable: "ModuleStages",
                        principalColumn: "StageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowMasters_WorkflowMasterId",
                        column: x => x.WorkflowMasterId,
                        principalSchema: "Workflow",
                        principalTable: "WorkflowMasters",
                        principalColumn: "WorkflowMasterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 168, null, "WorkflowRuntime" },
                    { 169, 168, "WorkflowModuleItemMapping.View" },
                    { 170, 168, "WorkflowModuleItemMapping.Create" },
                    { 171, 168, "WorkflowModuleItemMapping.Update" },
                    { 172, 168, "WorkflowModuleItemMapping.Delete" },
                    { 173, 168, "WorkflowRuntime.View" },
                    { 174, 168, "WorkflowRuntime.Perform" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleItemMappings_ModuleId",
                schema: "Workflow",
                table: "ModuleItemMappings",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleItemMappings_TenantId_ReferenceItemType",
                schema: "Workflow",
                table: "ModuleItemMappings",
                columns: new[] { "TenantId", "ReferenceItemType" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowMasters_ModuleId",
                schema: "Workflow",
                table: "WorkflowMasters",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowMasters_TenantId_ReferenceItemType_ReferenceItemId",
                schema: "Workflow",
                table: "WorkflowMasters",
                columns: new[] { "TenantId", "ReferenceItemType", "ReferenceItemId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ModuleStageId",
                schema: "Workflow",
                table: "WorkflowTasks",
                column: "ModuleStageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowMasterId_IsCompleted",
                schema: "Workflow",
                table: "WorkflowTasks",
                columns: new[] { "WorkflowMasterId", "IsCompleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleItemMappings",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "WorkflowTasks",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "WorkflowMasters",
                schema: "Workflow");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 169);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 170);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 171);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 172);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 173);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 174);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 168);
        }
    }
}
