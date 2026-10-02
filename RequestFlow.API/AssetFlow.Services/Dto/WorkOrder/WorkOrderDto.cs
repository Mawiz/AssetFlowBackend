using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.WorkOrder
{
    public class WorkOrderAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class WorkOrderStatusHistoryDto
    {
        public int Id { get; set; }
        public int FromStatus { get; set; }
        public int ToStatus { get; set; }
        public int ChangedByUserId { get; set; }
        public string ChangedByUserName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderAssignmentHistoryDto
    {
        public int Id { get; set; }
        public int AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public int AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderDiagnosisDto
    {
        public int Id { get; set; }
        public string InitialProblem { get; set; }
        public string Diagnosis { get; set; }
        public string RootCause { get; set; }
        public string ActionTaken { get; set; }
        public string FinalResult { get; set; }
        public int DiagnosedByUserId { get; set; }
        public string DiagnosedByUserName { get; set; }
        public DateTime DiagnosedAt { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string WorkOrderNumber { get; set; }
        public int SourceType { get; set; }
        public int? AssetIssueId { get; set; }
        public string IssueNumber { get; set; }
        public string IssueDescription { get; set; }
        public int? PreventiveMaintenanceOccurrenceId { get; set; }
        public string MaintenanceScheduleName { get; set; }
        public string MaintenanceTypeName { get; set; }
        public DateTime? PmScheduledDate { get; set; }
        public DateTime? PmDueDate { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int LocationId { get; set; }
        public string LocationDisplayPath { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? EngineerArrivedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? PausedAt { get; set; }
        public DateTime? ResumedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedByUserId { get; set; }
        public string CompletedByUserName { get; set; }
        public DateTime? AssetRestoredAt { get; set; }
        public int? AssetStatusAfterWork { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string WorkPerformed { get; set; }
        public string FinalResult { get; set; }
        public string Remarks { get; set; }
        public string RejectionReason { get; set; }
        public string CancellationReason { get; set; }
        public string ApprovalRemarks { get; set; }
        public int? ApprovedByUserId { get; set; }
        public string ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string IssueReportedByUserName { get; set; }
        public DateTime? IssueReportedAt { get; set; }
        public string IssueImmediateAction { get; set; }
        public WorkOrderDiagnosisDto Diagnosis { get; set; }
        public List<WorkOrderStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<WorkOrderAssignmentHistoryDto> AssignmentHistory { get; set; } = new();
        public List<WorkOrderAttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateWorkOrderDto
    {
        public int? TenantId { get; set; }
        public int SourceType { get; set; }
        public int? AssetIssueId { get; set; }
        public int? PreventiveMaintenanceOccurrenceId { get; set; }
        public int AssetId { get; set; }
        public int Priority { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateWorkOrderFromIssueDto
    {
        public int? TenantId { get; set; }
        public int AssetIssueId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateWorkOrderFromOccurrenceDto
    {
        public int? TenantId { get; set; }
        public int PreventiveMaintenanceOccurrenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateWorkOrderDto
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class WorkOrderAssignDto
    {
        public int Id { get; set; }
        public int AssignedToUserId { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderActionRemarksDto
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderEngineerArrivedDto
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderDiagnosisUpsertDto
    {
        public int WorkOrderId { get; set; }
        public string InitialProblem { get; set; }
        public string Diagnosis { get; set; }
        public string RootCause { get; set; }
        public string ActionTaken { get; set; }
        public string FinalResult { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderWorkPerformedDto
    {
        public int Id { get; set; }
        public string WorkPerformed { get; set; }
        public string FinalResult { get; set; }
        public string Remarks { get; set; }
        public int? AssetStatusAfterWork { get; set; }
    }

    public class WorkOrderCompleteDto
    {
        public int Id { get; set; }
        public string WorkPerformed { get; set; }
        public string FinalResult { get; set; }
        public string Remarks { get; set; }
        public int? AssetStatusAfterWork { get; set; }
    }

    public class WorkOrderApproveDto
    {
        public int Id { get; set; }
        public string ApprovalRemarks { get; set; }
        public bool RestoreAssetOperational { get; set; }
    }

    public class WorkOrderRejectDto
    {
        public int Id { get; set; }
        public string RejectionReason { get; set; }
    }

    public class WorkOrderReopenDto
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
    }

    public class WorkOrderCancelDto
    {
        public int Id { get; set; }
        public string CancellationReason { get; set; }
    }

    public class WorkOrderFilterDto : SearchViewDto
    {
        public int? SourceType { get; set; }
        public int? AssetId { get; set; }
        public int? AssetIssueId { get; set; }
        public int? PreventiveMaintenanceOccurrenceId { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public int? LocationId { get; set; }
        public int? AssignedToUserId { get; set; }
        public bool? MyAssignmentsOnly { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
