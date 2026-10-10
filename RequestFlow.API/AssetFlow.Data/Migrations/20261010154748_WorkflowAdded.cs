using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Workflow");

            migrationBuilder.CreateTable(
                name: "Actions",
                schema: "Workflow",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeforeActionEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BeforeActionAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AfterActionEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AfterActionAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BaseCssClass = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsNotesRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.ActionId);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "Workflow",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    ModuleNameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModuleNameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsReOpen = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Modules", x => x.ModuleId);
                    table.ForeignKey(
                        name: "FK_Modules_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModuleStages",
                schema: "Workflow",
                columns: table => new
                {
                    StageId = table.Column<int>(type: "int", nullable: false),
                    StageNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StageNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    DueInHours = table.Column<int>(type: "int", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: false),
                    IsFinalApprover = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgementTemplateId = table.Column<int>(type: "int", nullable: true),
                    IsClosingStage = table.Column<bool>(type: "bit", nullable: false),
                    IsLogOpen = table.Column<bool>(type: "bit", nullable: false),
                    IsTATExtension = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleStages", x => x.StageId);
                    table.ForeignKey(
                        name: "FK_ModuleStages_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "Workflow",
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageActionViewer",
                schema: "Workflow",
                columns: table => new
                {
                    StageActionViewerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    ViewerTypeId = table.Column<int>(type: "int", nullable: false),
                    ViewerValue = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageActionViewer", x => x.StageActionViewerId);
                    table.ForeignKey(
                        name: "FK_StageActionViewer_ModuleStages_StageId",
                        column: x => x.StageId,
                        principalSchema: "Workflow",
                        principalTable: "ModuleStages",
                        principalColumn: "StageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagesActions",
                schema: "Workflow",
                columns: table => new
                {
                    StageActionId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    StageActionDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StageActionDisplayNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrderNo = table.Column<int>(type: "int", nullable: false),
                    CssClass = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NextSageId = table.Column<int>(type: "int", nullable: true),
                    AssigneeTypeId = table.Column<int>(type: "int", nullable: true),
                    AssignmentValue = table.Column<int>(type: "int", nullable: true),
                    AcknowledgementTemplateId = table.Column<int>(type: "int", nullable: true),
                    ActionPopUpRenderingComponents = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SMSTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagesActions", x => x.StageActionId);
                    table.ForeignKey(
                        name: "FK_StagesActions_Actions_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "Workflow",
                        principalTable: "Actions",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StagesActions_ModuleStages_StageId",
                        column: x => x.StageId,
                        principalSchema: "Workflow",
                        principalTable: "ModuleStages",
                        principalColumn: "StageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 163, null, "WorkflowConfiguration" },
                    { 164, 163, "WorkflowConfiguration.View" },
                    { 165, 163, "WorkflowConfiguration.Create" },
                    { 166, 163, "WorkflowConfiguration.Update" },
                    { 167, 163, "WorkflowConfiguration.Delete" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_TenantId_ModuleNameEn",
                schema: "Workflow",
                table: "Modules",
                columns: new[] { "TenantId", "ModuleNameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleStages_ModuleId_SequenceNo_VersionNo",
                schema: "Workflow",
                table: "ModuleStages",
                columns: new[] { "ModuleId", "SequenceNo", "VersionNo" });

            migrationBuilder.CreateIndex(
                name: "IX_StageActionViewer_StageId_ViewerTypeId_ViewerValue",
                schema: "Workflow",
                table: "StageActionViewer",
                columns: new[] { "StageId", "ViewerTypeId", "ViewerValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagesActions_ActionId",
                schema: "Workflow",
                table: "StagesActions",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_StagesActions_StageId_DisplayOrderNo",
                schema: "Workflow",
                table: "StagesActions",
                columns: new[] { "StageId", "DisplayOrderNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StageActionViewer",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "StagesActions",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "Actions",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "ModuleStages",
                schema: "Workflow");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "Workflow");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 167);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 163);
        }
    }
}
