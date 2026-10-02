using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderStarted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    AssetIssueId = table.Column<int>(type: "int", nullable: true),
                    PreventiveMaintenanceOccurrenceId = table.Column<int>(type: "int", nullable: true),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationDisplayPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EngineerArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PausedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResumedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssetRestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetStatusAfterWork = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkPerformed = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FinalResult = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrders_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrders_AssetIssues_AssetIssueId",
                        column: x => x.AssetIssueId,
                        principalTable: "AssetIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_PreventiveMaintenanceOccurrences_PreventiveMaintenanceOccurrenceId",
                        column: x => x.PreventiveMaintenanceOccurrenceId,
                        principalTable: "PreventiveMaintenanceOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderAssignmentHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnassignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderAssignmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderAssignmentHistories_ApplicationUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderAssignmentHistories_ApplicationUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderAssignmentHistories_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderAssignmentHistories_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderAssignmentHistories_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderAttachments_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderAttachments_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderAttachments_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderDiagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    InitialProblem = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ActionTaken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FinalResult = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DiagnosedByUserId = table.Column<int>(type: "int", nullable: false),
                    DiagnosedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderDiagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderDiagnoses_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderDiagnoses_ApplicationUsers_DiagnosedByUserId",
                        column: x => x.DiagnosedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderDiagnoses_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderDiagnoses_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderStatusHistories_ApplicationUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderStatusHistories_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderStatusHistories_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderStatusHistories_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "FeatureId", "ResourceName" },
                values: new object[,]
                {
                    { 122, null, "WorkOrder" },
                    { 136, 122, "WorkOrder.View" },
                    { 137, 122, "WorkOrder.Create" },
                    { 138, 122, "WorkOrder.Update" },
                    { 139, 122, "WorkOrder.Delete" },
                    { 140, 122, "WorkOrder.Assign" },
                    { 141, 122, "WorkOrder.Reassign" },
                    { 142, 122, "WorkOrder.Accept" },
                    { 143, 122, "WorkOrder.Start" },
                    { 144, 122, "WorkOrder.Pause" },
                    { 145, 122, "WorkOrder.Resume" },
                    { 146, 122, "WorkOrder.Complete" },
                    { 147, 122, "WorkOrder.Approve" },
                    { 148, 122, "WorkOrder.Reject" },
                    { 149, 122, "WorkOrder.Reopen" },
                    { 150, 122, "WorkOrder.Cancel" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAssignmentHistories_AssignedByUserId",
                table: "WorkOrderAssignmentHistories",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAssignmentHistories_AssignedToUserId",
                table: "WorkOrderAssignmentHistories",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAssignmentHistories_CreatedById",
                table: "WorkOrderAssignmentHistories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAssignmentHistories_ModifiedById",
                table: "WorkOrderAssignmentHistories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAssignmentHistories_WorkOrderId",
                table: "WorkOrderAssignmentHistories",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAttachments_CreatedById",
                table: "WorkOrderAttachments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAttachments_ModifiedById",
                table: "WorkOrderAttachments",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAttachments_WorkOrderId",
                table: "WorkOrderAttachments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDiagnoses_CreatedById",
                table: "WorkOrderDiagnoses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDiagnoses_DiagnosedByUserId",
                table: "WorkOrderDiagnoses",
                column: "DiagnosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDiagnoses_ModifiedById",
                table: "WorkOrderDiagnoses",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderDiagnoses_WorkOrderId",
                table: "WorkOrderDiagnoses",
                column: "WorkOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ApprovedByUserId",
                table: "WorkOrders",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssetId",
                table: "WorkOrders",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssetIssueId",
                table: "WorkOrders",
                column: "AssetIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedByUserId",
                table: "WorkOrders",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedToUserId",
                table: "WorkOrders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CompletedByUserId",
                table: "WorkOrders",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedById",
                table: "WorkOrders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_LocationId",
                table: "WorkOrders",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ModifiedById",
                table: "WorkOrders",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_PreventiveMaintenanceOccurrenceId",
                table: "WorkOrders",
                column: "PreventiveMaintenanceOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                table: "WorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_TenantId_WorkOrderNumber",
                table: "WorkOrders",
                columns: new[] { "TenantId", "WorkOrderNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusHistories_ChangedByUserId",
                table: "WorkOrderStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusHistories_CreatedById",
                table: "WorkOrderStatusHistories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusHistories_ModifiedById",
                table: "WorkOrderStatusHistories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusHistories_WorkOrderId",
                table: "WorkOrderStatusHistories",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderAssignmentHistories");

            migrationBuilder.DropTable(
                name: "WorkOrderAttachments");

            migrationBuilder.DropTable(
                name: "WorkOrderDiagnoses");

            migrationBuilder.DropTable(
                name: "WorkOrderStatusHistories");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 150);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 122);
        }
    }
}
