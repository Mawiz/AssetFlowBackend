using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTypes_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceTypes_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceChecklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MaintenanceTypeId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklists_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklists_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklists_MaintenanceTypes_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "MaintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklists_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceChecklistId = table.Column<int>(type: "int", nullable: false),
                    ItemText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponseType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItems_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItems_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItems_MaintenanceChecklists_MaintenanceChecklistId",
                        column: x => x.MaintenanceChecklistId,
                        principalTable: "MaintenanceChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceTypeId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceChecklistId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    IntervalValue = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextDueOperatingHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NextDueCycles = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastOccurrenceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    GenerationHorizonDays = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_ApplicationUsers_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_MaintenanceChecklists_MaintenanceChecklistId",
                        column: x => x.MaintenanceChecklistId,
                        principalTable: "MaintenanceChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_MaintenanceTypes_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "MaintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceChecklistItemOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceChecklistItemId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceChecklistItemOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItemOptions_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItemOptions_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceChecklistItemOptions_MaintenanceChecklistItems_MaintenanceChecklistItemId",
                        column: x => x.MaintenanceChecklistItemId,
                        principalTable: "MaintenanceChecklistItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveMaintenanceOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceScheduleId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceTypeId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceChecklistId = table.Column<int>(type: "int", nullable: true),
                    ChecklistVersion = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueOperatingHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DueCycles = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveMaintenanceOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_ApplicationUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_ApplicationUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_MaintenanceChecklists_MaintenanceChecklistId",
                        column: x => x.MaintenanceChecklistId,
                        principalTable: "MaintenanceChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_MaintenanceSchedules_MaintenanceScheduleId",
                        column: x => x.MaintenanceScheduleId,
                        principalTable: "MaintenanceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_MaintenanceTypes_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "MaintenanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PreventiveMaintenanceOccurrenceChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveMaintenanceOccurrenceId = table.Column<int>(type: "int", nullable: false),
                    SourceChecklistItemId = table.Column<int>(type: "int", nullable: true),
                    ItemText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponseType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveMaintenanceOccurrenceChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrenceChecklistItems_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrenceChecklistItems_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceOccurrenceChecklistItems_PreventiveMaintenanceOccurrences_PreventiveMaintenanceOccurrenceId",
                        column: x => x.PreventiveMaintenanceOccurrenceId,
                        principalTable: "PreventiveMaintenanceOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveMaintenanceChecklistResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveMaintenanceOccurrenceId = table.Column<int>(type: "int", nullable: false),
                    OccurrenceChecklistItemId = table.Column<int>(type: "int", nullable: false),
                    ResponseValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveMaintenanceChecklistResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceChecklistResponses_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceChecklistResponses_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceChecklistResponses_PreventiveMaintenanceOccurrenceChecklistItems_OccurrenceChecklistItemId",
                        column: x => x.OccurrenceChecklistItemId,
                        principalTable: "PreventiveMaintenanceOccurrenceChecklistItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveMaintenanceChecklistResponses_PreventiveMaintenanceOccurrences_PreventiveMaintenanceOccurrenceId",
                        column: x => x.PreventiveMaintenanceOccurrenceId,
                        principalTable: "PreventiveMaintenanceOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 100, null, "MaintenanceType" },
                    { 101, null, "MaintenanceChecklist" },
                    { 102, null, "MaintenanceSchedule" },
                    { 103, null, "PreventiveMaintenance" },
                    { 104, 100, "MaintenanceType.View" },
                    { 105, 100, "MaintenanceType.Create" },
                    { 106, 100, "MaintenanceType.Update" },
                    { 107, 100, "MaintenanceType.Delete" },
                    { 108, 101, "MaintenanceChecklist.View" },
                    { 109, 101, "MaintenanceChecklist.Create" },
                    { 110, 101, "MaintenanceChecklist.Update" },
                    { 111, 101, "MaintenanceChecklist.Delete" },
                    { 112, 102, "MaintenanceSchedule.View" },
                    { 113, 102, "MaintenanceSchedule.Create" },
                    { 114, 102, "MaintenanceSchedule.Update" },
                    { 115, 102, "MaintenanceSchedule.Delete" },
                    { 116, 103, "PreventiveMaintenance.View" },
                    { 117, 103, "PreventiveMaintenance.Update" },
                    { 118, 103, "PreventiveMaintenance.Complete" },
                    { 119, 103, "PreventiveMaintenance.Generate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItemOptions_CreatedById",
                table: "MaintenanceChecklistItemOptions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItemOptions_MaintenanceChecklistItemId",
                table: "MaintenanceChecklistItemOptions",
                column: "MaintenanceChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItemOptions_ModifiedById",
                table: "MaintenanceChecklistItemOptions",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItems_CreatedById",
                table: "MaintenanceChecklistItems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItems_MaintenanceChecklistId",
                table: "MaintenanceChecklistItems",
                column: "MaintenanceChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklistItems_ModifiedById",
                table: "MaintenanceChecklistItems",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklists_CreatedById",
                table: "MaintenanceChecklists",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklists_MaintenanceTypeId",
                table: "MaintenanceChecklists",
                column: "MaintenanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklists_ModifiedById",
                table: "MaintenanceChecklists",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceChecklists_TenantId_Code",
                table: "MaintenanceChecklists",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_AssetId",
                table: "MaintenanceSchedules",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_CreatedById",
                table: "MaintenanceSchedules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MaintenanceChecklistId",
                table: "MaintenanceSchedules",
                column: "MaintenanceChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MaintenanceTypeId",
                table: "MaintenanceSchedules",
                column: "MaintenanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_ModifiedById",
                table: "MaintenanceSchedules",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_ResponsibleUserId",
                table: "MaintenanceSchedules",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_TenantId",
                table: "MaintenanceSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTypes_CreatedById",
                table: "MaintenanceTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTypes_ModifiedById",
                table: "MaintenanceTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTypes_TenantId_Code",
                table: "MaintenanceTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceChecklistResponses_CreatedById",
                table: "PreventiveMaintenanceChecklistResponses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceChecklistResponses_ModifiedById",
                table: "PreventiveMaintenanceChecklistResponses",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceChecklistResponses_OccurrenceChecklistItemId",
                table: "PreventiveMaintenanceChecklistResponses",
                column: "OccurrenceChecklistItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceChecklistResponses_PreventiveMaintenanceOccurrenceId",
                table: "PreventiveMaintenanceChecklistResponses",
                column: "PreventiveMaintenanceOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrenceChecklistItems_CreatedById",
                table: "PreventiveMaintenanceOccurrenceChecklistItems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrenceChecklistItems_ModifiedById",
                table: "PreventiveMaintenanceOccurrenceChecklistItems",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrenceChecklistItems_PreventiveMaintenanceOccurrenceId",
                table: "PreventiveMaintenanceOccurrenceChecklistItems",
                column: "PreventiveMaintenanceOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_AssetId",
                table: "PreventiveMaintenanceOccurrences",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_CompletedByUserId",
                table: "PreventiveMaintenanceOccurrences",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_CreatedById",
                table: "PreventiveMaintenanceOccurrences",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_MaintenanceChecklistId",
                table: "PreventiveMaintenanceOccurrences",
                column: "MaintenanceChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_MaintenanceScheduleId",
                table: "PreventiveMaintenanceOccurrences",
                column: "MaintenanceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_MaintenanceTypeId",
                table: "PreventiveMaintenanceOccurrences",
                column: "MaintenanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_ModifiedById",
                table: "PreventiveMaintenanceOccurrences",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_StartedByUserId",
                table: "PreventiveMaintenanceOccurrences",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_TenantId_MaintenanceScheduleId_DueCycles",
                table: "PreventiveMaintenanceOccurrences",
                columns: new[] { "TenantId", "MaintenanceScheduleId", "DueCycles" },
                unique: true,
                filter: "[DueCycles] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_TenantId_MaintenanceScheduleId_DueOperatingHours",
                table: "PreventiveMaintenanceOccurrences",
                columns: new[] { "TenantId", "MaintenanceScheduleId", "DueOperatingHours" },
                unique: true,
                filter: "[DueOperatingHours] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveMaintenanceOccurrences_TenantId_MaintenanceScheduleId_ScheduledDate",
                table: "PreventiveMaintenanceOccurrences",
                columns: new[] { "TenantId", "MaintenanceScheduleId", "ScheduledDate" },
                unique: true,
                filter: "[ScheduledDate] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceChecklistItemOptions");

            migrationBuilder.DropTable(
                name: "PreventiveMaintenanceChecklistResponses");

            migrationBuilder.DropTable(
                name: "MaintenanceChecklistItems");

            migrationBuilder.DropTable(
                name: "PreventiveMaintenanceOccurrenceChecklistItems");

            migrationBuilder.DropTable(
                name: "PreventiveMaintenanceOccurrences");

            migrationBuilder.DropTable(
                name: "MaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "MaintenanceChecklists");

            migrationBuilder.DropTable(
                name: "MaintenanceTypes");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 103);
        }
    }
}
