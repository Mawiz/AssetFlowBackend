using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.WorkOrder;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Core.Issue;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.WorkOrder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;

namespace AssetFlow.Services.Core.WorkOrder
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;

        public WorkOrderService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor,
            IHostEnvironment environment)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public Task<ResponseDto<WorkOrderDto>> CreateFromIssueAsync(CreateWorkOrderFromIssueDto dto) =>
            CreateFromIssueInternalAsync(dto);

        public Task<ResponseDto<WorkOrderDto>> CreateFromOccurrenceAsync(CreateWorkOrderFromOccurrenceDto dto) =>
            CreateFromOccurrenceInternalAsync(dto);

        private async Task<ResponseDto<WorkOrderDto>> CreateFromIssueInternalAsync(CreateWorkOrderFromIssueDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var issue = await _context.AssetIssues.FindAsync(dto.AssetIssueId);
            if (issue == null || !issue.IsActive)
            { response.AddError("Issue not found."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, issue.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? issue.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (issue.TenantId != tenantResult.TenantId)
            { response.AddError("Issue does not belong to the selected tenant."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (issue.WorkOrderId.HasValue && await HasActiveWorkOrderForIssueAsync(issue.Id, issue.WorkOrderId))
            { response.AddError("An active work order already exists for this issue."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            var title = string.IsNullOrWhiteSpace(dto.Title)
                ? Truncate(issue.Description, 200)
                : dto.Title.Trim();
            var desc = string.IsNullOrWhiteSpace(dto.Description) ? issue.Description : dto.Description.Trim();

            var wo = await BuildWorkOrderAsync(
                tenantResult.TenantId,
                (int)Enums.WorkOrderSourceType.Issue,
                issue.AssetId,
                issue.Priority,
                title,
                desc,
                dto.DueDate,
                issueId: issue.Id,
                occurrenceId: null);

            issue.WorkOrderId = wo.Id;
            await TransitionAsync(wo, (int)Enums.WorkOrderStatus.PendingAssignment, "Created from issue", false);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(wo.Id);
            return response;
        }

        private async Task<ResponseDto<WorkOrderDto>> CreateFromOccurrenceInternalAsync(CreateWorkOrderFromOccurrenceDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var occ = await _context.PreventiveMaintenanceOccurrences
                .Include(x => x.MaintenanceSchedule)
                .FirstOrDefaultAsync(x => x.Id == dto.PreventiveMaintenanceOccurrenceId);
            if (occ == null || !occ.IsActive)
            { response.AddError("PM occurrence not found."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, occ.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? occ.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (occ.TenantId != tenantResult.TenantId)
            { response.AddError("PM occurrence does not belong to the selected tenant."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (occ.WorkOrderId.HasValue && await HasActiveWorkOrderForOccurrenceAsync(occ.Id, occ.WorkOrderId))
            { response.AddError("An active work order already exists for this PM occurrence."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            var scheduleName = occ.MaintenanceSchedule?.Name ?? "Preventive Maintenance";
            var title = string.IsNullOrWhiteSpace(dto.Title) ? scheduleName : dto.Title.Trim();
            var desc = string.IsNullOrWhiteSpace(dto.Description) ? scheduleName : dto.Description.Trim();

            var wo = await BuildWorkOrderAsync(
                tenantResult.TenantId,
                (int)Enums.WorkOrderSourceType.PreventiveMaintenance,
                occ.AssetId,
                (int)Enums.IssuePriority.Medium,
                title,
                desc,
                dto.DueDate ?? occ.DueDate,
                issueId: null,
                occurrenceId: occ.Id);

            occ.WorkOrderId = wo.Id;
            await TransitionAsync(wo, (int)Enums.WorkOrderStatus.PendingAssignment, "Created from PM occurrence", false);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(wo.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> UpdateAsync(UpdateWorkOrderDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAndEditableAsync(entity, response)) return response;

            entity.Priority = dto.Priority;
            entity.Title = dto.Title?.Trim() ?? string.Empty;
            entity.Description = dto.Description?.Trim() ?? string.Empty;
            entity.DueDate = dto.DueDate;
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var dto = await MapDetailAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(WorkOrderFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, model.TenantId);
            query = query
                .Include(x => x.Asset)
                .Include(x => x.Location)
                .Include(x => x.AssignedToUser)
                .Include(x => x.AssetIssue)
                .Include(x => x.PreventiveMaintenanceOccurrence)
                .Include(x => x.Tenant);

            if (model.SourceType.HasValue) query = query.Where(x => x.SourceType == model.SourceType);
            if (model.AssetId.HasValue) query = query.Where(x => x.AssetId == model.AssetId);
            if (model.AssetIssueId.HasValue) query = query.Where(x => x.AssetIssueId == model.AssetIssueId);
            if (model.PreventiveMaintenanceOccurrenceId.HasValue)
                query = query.Where(x => x.PreventiveMaintenanceOccurrenceId == model.PreventiveMaintenanceOccurrenceId);
            if (model.Priority.HasValue) query = query.Where(x => x.Priority == model.Priority);
            if (model.Status.HasValue) query = query.Where(x => x.Status == model.Status);
            if (model.LocationId.HasValue) query = query.Where(x => x.LocationId == model.LocationId);
            if (model.AssignedToUserId.HasValue) query = query.Where(x => x.AssignedToUserId == model.AssignedToUserId);
            if (model.MyAssignmentsOnly == true)
            {
                var uid = UserHelper.GetCurrentUserId(_httpContextAccessor);
                if (uid.HasValue) query = query.Where(x => x.AssignedToUserId == uid);
            }
            if (model.CreatedFrom.HasValue) query = query.Where(x => x.CreatedOn >= model.CreatedFrom);
            if (model.CreatedTo.HasValue) query = query.Where(x => x.CreatedOn <= model.CreatedTo);
            if (model.StartDate.HasValue) query = query.Where(x => x.CreatedOn >= model.StartDate);
            if (model.EndDate.HasValue) query = query.Where(x => x.CreatedOn <= model.EndDate);

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x =>
                    x.WorkOrderNumber.Contains(s) ||
                    x.Title.Contains(s) ||
                    x.Description.Contains(s) ||
                    x.Asset.AssetCode.Contains(s) ||
                    x.Asset.Name.Contains(s) ||
                    (x.AssetIssue != null && x.AssetIssue.IssueNumber.Contains(s)) ||
                    (x.AssignedToUser != null && (x.AssignedToUser.FullName.Contains(s) || x.AssignedToUser.UserName.Contains(s))));
            }

            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "CreatedOn" : model.OrderByProp;
            if (model.SortDirection == (int)Enums.OrderBy.Descending) order += " descending";
            var page = await query.OrderBy(order).Select(ProjectList()).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.WorkOrders.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (entity.Status == (int)Enums.WorkOrderStatus.Closed)
            { response.AddError("Closed work orders cannot be deleted."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> AssignAsync(WorkOrderAssignDto dto) =>
            await AssignInternalAsync(dto, isReassign: false);

        public async Task<ResponseDto<WorkOrderDto>> ReassignAsync(WorkOrderAssignDto dto) =>
            await AssignInternalAsync(dto, isReassign: true);

        private async Task<ResponseDto<WorkOrderDto>> AssignInternalAsync(WorkOrderAssignDto dto, bool isReassign)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAsync(entity, response)) return response;

            var userError = await ValidateAssigneeAsync(entity.TenantId, dto.AssignedToUserId);
            if (userError != null) { response.AddError(userError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            if (isReassign)
            {
                if (from != (int)Enums.WorkOrderStatus.Assigned && from != (int)Enums.WorkOrderStatus.Accepted)
                { response.AddError("Work order cannot be reassigned in the current status."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                await CloseCurrentAssignmentAsync(entity.Id);
                entity.Status = (int)Enums.WorkOrderStatus.Assigned;
                entity.AcceptedAt = null;
                entity.StartedAt = null;
            }
            else
            {
                if (!WorkOrderTransitionHelper.CanTransition(from, (int)Enums.WorkOrderStatus.Assigned) &&
                    from != (int)Enums.WorkOrderStatus.PendingAssignment)
                { response.AddError("Work order cannot be assigned in the current status."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                entity.Status = (int)Enums.WorkOrderStatus.Assigned;
            }

            var managerId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            entity.AssignedByUserId = managerId;
            entity.AssignedToUserId = dto.AssignedToUserId;
            entity.AssignedAt = DateTime.UtcNow;

            _context.WorkOrderAssignmentHistories.Add(new WorkOrderAssignmentHistory
            {
                WorkOrderId = entity.Id,
                AssignedToUserId = dto.AssignedToUserId,
                AssignedByUserId = managerId ?? 0,
                AssignedAt = entity.AssignedAt.Value,
                Remarks = dto.Remarks?.Trim() ?? string.Empty
            });

            await AppendStatusHistoryAsync(entity, from, entity.Status, isReassign ? "Reassigned" : "Assigned", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> AcceptAsync(WorkOrderActionRemarksDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.Assigned)
            { response.AddError("Work order must be assigned before acceptance."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.Accepted;
            entity.AcceptedAt = DateTime.UtcNow;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Accepted", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> EngineerArrivedAsync(WorkOrderEngineerArrivedDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            entity.EngineerArrivedAt = DateTime.UtcNow;
            await AppendStatusHistoryAsync(entity, entity.Status, entity.Status, "Engineer arrived", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> StartAsync(WorkOrderActionRemarksDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.Accepted && entity.Status != (int)Enums.WorkOrderStatus.Reopened)
            { response.AddError("Work order must be accepted before starting."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.InProgress;
            entity.StartedAt ??= DateTime.UtcNow;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Work started", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> PauseAsync(WorkOrderActionRemarksDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.InProgress)
            { response.AddError("Only in-progress work can be paused."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            entity.PausedAt = DateTime.UtcNow;
            await AppendStatusHistoryAsync(entity, entity.Status, entity.Status, "Paused", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> WaitingForPartsAsync(WorkOrderActionRemarksDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.InProgress)
            { response.AddError("Work order must be in progress."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.WaitingForParts;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Waiting for parts", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> ResumeAsync(WorkOrderActionRemarksDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.InProgress && entity.Status != (int)Enums.WorkOrderStatus.WaitingForParts)
            { response.AddError("Work order cannot be resumed in the current status."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (entity.Status == (int)Enums.WorkOrderStatus.WaitingForParts)
            {
                var from = entity.Status;
                entity.Status = (int)Enums.WorkOrderStatus.InProgress;
                await AppendStatusHistoryAsync(entity, from, entity.Status, "Resumed from waiting for parts", dto.Remarks);
            }
            else
            {
                await AppendStatusHistoryAsync(entity, entity.Status, entity.Status, "Resumed", dto.Remarks);
            }
            entity.ResumedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> UpsertDiagnosisAsync(WorkOrderDiagnosisUpsertDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.WorkOrderId, response);
            if (entity == null) return response;
            if (WorkOrderTransitionHelper.IsTerminal(entity.Status))
            { response.AddError("Closed or cancelled work orders cannot be updated."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor) ?? 0;
            var existing = await _context.WorkOrderDiagnoses.FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId);
            if (existing == null)
            {
                existing = new WorkOrderDiagnosis { WorkOrderId = dto.WorkOrderId };
                _context.WorkOrderDiagnoses.Add(existing);
            }
            existing.InitialProblem = dto.InitialProblem?.Trim() ?? string.Empty;
            existing.Diagnosis = dto.Diagnosis?.Trim() ?? string.Empty;
            existing.RootCause = dto.RootCause?.Trim() ?? string.Empty;
            existing.ActionTaken = dto.ActionTaken?.Trim() ?? string.Empty;
            existing.FinalResult = dto.FinalResult?.Trim() ?? string.Empty;
            existing.Remarks = dto.Remarks?.Trim() ?? string.Empty;
            existing.DiagnosedByUserId = userId;
            existing.DiagnosedAt = DateTime.UtcNow;

            await AppendStatusHistoryAsync(entity, entity.Status, entity.Status, "Diagnosis updated", null);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> UpdateWorkPerformedAsync(WorkOrderWorkPerformedDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (WorkOrderTransitionHelper.IsTerminal(entity.Status))
            { response.AddError("Closed or cancelled work orders cannot be updated."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.WorkPerformed = dto.WorkPerformed?.Trim() ?? string.Empty;
            entity.FinalResult = dto.FinalResult?.Trim() ?? string.Empty;
            entity.Remarks = dto.Remarks?.Trim() ?? entity.Remarks;
            if (dto.AssetStatusAfterWork.HasValue) entity.AssetStatusAfterWork = dto.AssetStatusAfterWork;
            await AppendStatusHistoryAsync(entity, entity.Status, entity.Status, "Work performed updated", null);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> CompleteAsync(WorkOrderCompleteDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await LoadForEngineerActionAsync(dto.Id, response);
            if (entity == null) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.InProgress && entity.Status != (int)Enums.WorkOrderStatus.WaitingForParts)
            { response.AddError("Work order must be in progress to complete."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var diagnosis = await _context.WorkOrderDiagnoses.FirstOrDefaultAsync(x => x.WorkOrderId == entity.Id);
            if (diagnosis == null || string.IsNullOrWhiteSpace(diagnosis.Diagnosis))
            { response.AddError("Diagnosis is required before completion."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.WorkPerformed = dto.WorkPerformed?.Trim() ?? entity.WorkPerformed;
            entity.FinalResult = dto.FinalResult?.Trim() ?? entity.FinalResult;
            entity.Remarks = dto.Remarks?.Trim() ?? entity.Remarks;
            if (dto.AssetStatusAfterWork.HasValue) entity.AssetStatusAfterWork = dto.AssetStatusAfterWork;

            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            entity.CompletedAt = DateTime.UtcNow;
            entity.CompletedByUserId = userId;

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.WaitingForApproval;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Completed — awaiting approval", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> ApproveAsync(WorkOrderApproveDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAsync(entity, response)) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.WaitingForApproval)
            { response.AddError("Work order is not awaiting approval."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var approverId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (entity.CompletedByUserId.HasValue && approverId == entity.CompletedByUserId)
            { response.AddError("The completing engineer cannot approve the same work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.Closed;
            entity.ApprovedByUserId = approverId;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovalRemarks = dto.ApprovalRemarks?.Trim() ?? string.Empty;
            entity.ClosedAt = DateTime.UtcNow;
            entity.AssetRestoredAt ??= DateTime.UtcNow;

            if (dto.RestoreAssetOperational)
            {
                var asset = await _context.Assets.FindAsync(entity.AssetId);
                if (asset != null)
                    asset.Status = (int)Enums.AssetStatus.Operational;
            }

            await AppendStatusHistoryAsync(entity, from, entity.Status, "Approved and closed", dto.ApprovalRemarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> RejectAsync(WorkOrderRejectDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAsync(entity, response)) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.WaitingForApproval)
            { response.AddError("Work order is not awaiting approval."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.Rejected;
            entity.RejectionReason = dto.RejectionReason?.Trim() ?? string.Empty;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Rejected", dto.RejectionReason);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> ReopenAsync(WorkOrderReopenDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAsync(entity, response)) return response;
            if (entity.Status != (int)Enums.WorkOrderStatus.Rejected)
            { response.AddError("Only rejected work orders can be reopened."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.Reopened;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Reopened", dto.Remarks);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderDto>> CancelAsync(WorkOrderCancelDto dto)
        {
            var response = new ResponseDto<WorkOrderDto>();
            var entity = await _context.WorkOrders.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!await EnsureAccessAsync(entity, response)) return response;
            if (WorkOrderTransitionHelper.IsTerminal(entity.Status))
            { response.AddError("Work order is already closed or cancelled."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var from = entity.Status;
            entity.Status = (int)Enums.WorkOrderStatus.Cancelled;
            entity.CancellationReason = dto.CancellationReason?.Trim() ?? string.Empty;
            await AppendStatusHistoryAsync(entity, from, entity.Status, "Cancelled", dto.CancellationReason);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<WorkOrderAttachmentDto>> AddAttachmentAsync(int workOrderId, IFormFile file)
        {
            var response = new ResponseDto<WorkOrderAttachmentDto>();
            var wo = await _context.WorkOrders.FindAsync(workOrderId);
            if (wo == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, wo.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (WorkOrderTransitionHelper.IsTerminal(wo.Status))
            { response.AddError("Cannot add attachments to closed or cancelled work orders."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (file == null || file.Length == 0) { response.AddError("File is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantSegment = wo.TenantId?.ToString() ?? "system";
            var folder = Path.Combine(_environment.ContentRootPath, "uploads", "workorders", tenantSegment, workOrderId.ToString());
            Directory.CreateDirectory(folder);
            var safeName = Path.GetFileName(file.FileName);
            var storedName = $"{Guid.NewGuid():N}_{safeName}";
            var fullPath = Path.Combine(folder, storedName);
            await using (var stream = File.Create(fullPath))
                await file.CopyToAsync(stream);

            var relative = Path.Combine("uploads", "workorders", tenantSegment, workOrderId.ToString(), storedName).Replace('\\', '/');
            var attachment = new WorkOrderAttachment
            {
                WorkOrderId = workOrderId,
                FileName = safeName,
                ContentType = file.ContentType ?? "application/octet-stream",
                StoragePath = relative,
                FileSizeBytes = file.Length
            };
            _context.WorkOrderAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            response.Result = new WorkOrderAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                DownloadUrl = $"/api/workorder/attachments/{attachment.Id}/download"
            };
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAttachmentAsync(int attachmentId)
        {
            var response = new ResponseDto<bool>();
            var attachment = await _context.WorkOrderAttachments.Include(x => x.WorkOrder).FirstOrDefaultAsync(x => x.Id == attachmentId);
            if (attachment == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, attachment.WorkOrder.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            attachment.IsDeleted = true;
            attachment.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentStreamAsync(int attachmentId)
        {
            var attachment = await _context.WorkOrderAttachments.Include(x => x.WorkOrder).FirstOrDefaultAsync(x => x.Id == attachmentId && !x.IsDeleted);
            if (attachment == null) return null;
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, attachment.WorkOrder.TenantId);
            if (!access.Ok) return null;
            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath)) return null;
            return (File.OpenRead(fullPath), attachment.ContentType, attachment.FileName);
        }

        private async Task<WorkOrderEntity> BuildWorkOrderAsync(
            int? tenantId,
            int sourceType,
            int assetId,
            int priority,
            string title,
            string description,
            DateTime? dueDate,
            int? issueId,
            int? occurrenceId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || !asset.IsActive) throw new InvalidOperationException("Asset not found.");
            if (asset.TenantId != tenantId) throw new InvalidOperationException("Asset tenant mismatch.");

            var locationPath = await IssueLocationPathHelper.BuildPathAsync(_context, asset.LocationId);
            var entity = new WorkOrderEntity
            {
                TenantId = tenantId,
                WorkOrderNumber = await GenerateWorkOrderNumberAsync(tenantId),
                SourceType = sourceType,
                AssetIssueId = issueId,
                PreventiveMaintenanceOccurrenceId = occurrenceId,
                AssetId = assetId,
                LocationId = asset.LocationId,
                LocationDisplayPath = locationPath,
                Priority = priority,
                Status = (int)Enums.WorkOrderStatus.New,
                Title = title,
                Description = description,
                DueDate = dueDate
            };
            _context.WorkOrders.Add(entity);
            await _context.SaveChangesAsync();
            await AppendStatusHistoryAsync(entity, 0, entity.Status, "Created", null);
            await _context.SaveChangesAsync();
            return entity;
        }

        private async Task TransitionAsync(WorkOrderEntity entity, int toStatus, string remarks, bool save)
        {
            var from = entity.Status;
            if (!WorkOrderTransitionHelper.CanTransition(from, toStatus) && from != toStatus)
                throw new InvalidOperationException($"Invalid transition from {from} to {toStatus}.");
            entity.Status = toStatus;
            await AppendStatusHistoryAsync(entity, from, toStatus, remarks, remarks);
            if (save) await _context.SaveChangesAsync();
        }

        private async Task AppendStatusHistoryAsync(WorkOrderEntity entity, int fromStatus, int toStatus, string action, string? remarks)
        {
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor) ?? 0;
            _context.WorkOrderStatusHistories.Add(new WorkOrderStatusHistory
            {
                WorkOrderId = entity.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Remarks = string.IsNullOrWhiteSpace(remarks) ? action : remarks.Trim()
            });
            await Task.CompletedTask;
        }

        private async Task CloseCurrentAssignmentAsync(int workOrderId)
        {
            var open = await _context.WorkOrderAssignmentHistories
                .Where(x => x.WorkOrderId == workOrderId && x.UnassignedAt == null)
                .OrderByDescending(x => x.AssignedAt)
                .FirstOrDefaultAsync();
            if (open != null) open.UnassignedAt = DateTime.UtcNow;
        }

        private async Task<bool> HasActiveWorkOrderForIssueAsync(int issueId, int? linkedWoId)
        {
            if (!linkedWoId.HasValue) return false;
            var wo = await _context.WorkOrders.FindAsync(linkedWoId.Value);
            return wo != null && !wo.IsDeleted && WorkOrderTransitionHelper.IsActiveForSource(wo.Status);
        }

        private async Task<bool> HasActiveWorkOrderForOccurrenceAsync(int occurrenceId, int? linkedWoId)
        {
            if (!linkedWoId.HasValue) return false;
            var wo = await _context.WorkOrders.FindAsync(linkedWoId.Value);
            return wo != null && !wo.IsDeleted && WorkOrderTransitionHelper.IsActiveForSource(wo.Status);
        }

        private async Task<string?> ValidateAssigneeAsync(int? tenantId, int assigneeUserId)
        {
            var user = await _context.ApplicationUsers.FindAsync(assigneeUserId);
            if (user == null || !user.IsActive) return "Assignee not found or inactive.";
            if (tenantId.HasValue && user.TenantId != tenantId) return "Assignee does not belong to this tenant.";
            return null;
        }

        private async Task<WorkOrderEntity?> LoadForEngineerActionAsync(int id, ResponseDto<WorkOrderDto> response)
        {
            var entity = await _context.WorkOrders.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return null; }
            if (!await EnsureAccessAsync(entity, response)) return null;
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) { response.AddError("User context required."); response.StatusCode = HttpStatusCode.BadRequest; return null; }
            if (entity.AssignedToUserId != userId)
            { response.AddError("Only the assigned engineer can perform this action."); response.StatusCode = HttpStatusCode.Forbidden; return null; }
            return entity;
        }

        private async Task<bool> EnsureAccessAsync(WorkOrderEntity entity, ResponseDto<WorkOrderDto> response)
        {
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return false; }
            return true;
        }

        private async Task<bool> EnsureAccessAndEditableAsync(WorkOrderEntity entity, ResponseDto<WorkOrderDto> response)
        {
            if (!await EnsureAccessAsync(entity, response)) return false;
            if (WorkOrderTransitionHelper.IsTerminal(entity.Status))
            { response.AddError("Closed or cancelled work orders cannot be edited."); response.StatusCode = HttpStatusCode.BadRequest; return false; }
            return true;
        }

        private async Task<string> GenerateWorkOrderNumberAsync(int? tenantId)
        {
            const string prefix = "WO-";
            var last = await _context.WorkOrders
                .Where(x => x.TenantId == tenantId && x.WorkOrderNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.WorkOrderNumber)
                .FirstOrDefaultAsync();
            var next = 1;
            if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length && int.TryParse(last[prefix.Length..], out var n))
                next = n + 1;
            return $"{prefix}{next:D5}";
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) ? string.Empty : value.Length <= max ? value : value[..max];

        private static System.Linq.Expressions.Expression<Func<WorkOrderEntity, WorkOrderDto>> ProjectList() =>
            x => new WorkOrderDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                WorkOrderNumber = x.WorkOrderNumber,
                SourceType = x.SourceType,
                AssetIssueId = x.AssetIssueId,
                IssueNumber = x.AssetIssue != null ? x.AssetIssue.IssueNumber : null,
                PreventiveMaintenanceOccurrenceId = x.PreventiveMaintenanceOccurrenceId,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                LocationId = x.LocationId,
                LocationDisplayPath = x.LocationDisplayPath,
                Priority = x.Priority,
                Status = x.Status,
                Title = x.Title,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? (x.AssignedToUser.FullName ?? x.AssignedToUser.UserName) : null,
                DueDate = x.DueDate,
                CreatedOn = x.CreatedOn,
                CompletedAt = x.CompletedAt,
                ClosedAt = x.ClosedAt
            };

        private async Task<WorkOrderDto?> MapDetailAsync(int id)
        {
            return await _context.WorkOrders
                .Include(x => x.Asset)
                .Include(x => x.Location)
                .Include(x => x.Tenant)
                .Include(x => x.AssetIssue).ThenInclude(i => i!.ReportedByUser)
                .Include(x => x.PreventiveMaintenanceOccurrence).ThenInclude(o => o!.MaintenanceSchedule)
                .Include(x => x.PreventiveMaintenanceOccurrence).ThenInclude(o => o!.MaintenanceType)
                .Include(x => x.AssignedByUser)
                .Include(x => x.AssignedToUser)
                .Include(x => x.CompletedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.Diagnosis).ThenInclude(d => d!.DiagnosedByUser)
                .Include(x => x.Attachments)
                .Include(x => x.StatusHistory).ThenInclude(h => h.ChangedByUser)
                .Include(x => x.AssignmentHistory).ThenInclude(h => h.AssignedToUser)
                .Include(x => x.AssignmentHistory).ThenInclude(h => h.AssignedByUser)
                .Where(x => x.Id == id)
                .Select(x => new WorkOrderDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                    WorkOrderNumber = x.WorkOrderNumber,
                    SourceType = x.SourceType,
                    AssetIssueId = x.AssetIssueId,
                    IssueNumber = x.AssetIssue != null ? x.AssetIssue.IssueNumber : null,
                    IssueDescription = x.AssetIssue != null ? x.AssetIssue.Description : null,
                    IssueReportedByUserName = x.AssetIssue != null && x.AssetIssue.ReportedByUser != null
                        ? (x.AssetIssue.ReportedByUser.FullName ?? x.AssetIssue.ReportedByUser.UserName) : null,
                    IssueReportedAt = x.AssetIssue != null ? x.AssetIssue.ReportedAt : null,
                    IssueImmediateAction = x.AssetIssue != null ? x.AssetIssue.ImmediateAction : null,
                    PreventiveMaintenanceOccurrenceId = x.PreventiveMaintenanceOccurrenceId,
                    MaintenanceScheduleName = x.PreventiveMaintenanceOccurrence != null && x.PreventiveMaintenanceOccurrence.MaintenanceSchedule != null
                        ? x.PreventiveMaintenanceOccurrence.MaintenanceSchedule.Name : null,
                    MaintenanceTypeName = x.PreventiveMaintenanceOccurrence != null && x.PreventiveMaintenanceOccurrence.MaintenanceType != null
                        ? x.PreventiveMaintenanceOccurrence.MaintenanceType.Name : null,
                    PmScheduledDate = x.PreventiveMaintenanceOccurrence != null ? x.PreventiveMaintenanceOccurrence.ScheduledDate : null,
                    PmDueDate = x.PreventiveMaintenanceOccurrence != null ? x.PreventiveMaintenanceOccurrence.DueDate : null,
                    AssetId = x.AssetId,
                    AssetCode = x.Asset.AssetCode,
                    AssetName = x.Asset.Name,
                    LocationId = x.LocationId,
                    LocationDisplayPath = x.LocationDisplayPath,
                    Priority = x.Priority,
                    Status = x.Status,
                    Title = x.Title,
                    Description = x.Description,
                    AssignedByUserId = x.AssignedByUserId,
                    AssignedByUserName = x.AssignedByUser != null ? (x.AssignedByUser.FullName ?? x.AssignedByUser.UserName) : null,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUserName = x.AssignedToUser != null ? (x.AssignedToUser.FullName ?? x.AssignedToUser.UserName) : null,
                    AssignedAt = x.AssignedAt,
                    AcceptedAt = x.AcceptedAt,
                    EngineerArrivedAt = x.EngineerArrivedAt,
                    StartedAt = x.StartedAt,
                    PausedAt = x.PausedAt,
                    ResumedAt = x.ResumedAt,
                    CompletedAt = x.CompletedAt,
                    CompletedByUserId = x.CompletedByUserId,
                    CompletedByUserName = x.CompletedByUser != null ? (x.CompletedByUser.FullName ?? x.CompletedByUser.UserName) : null,
                    AssetRestoredAt = x.AssetRestoredAt,
                    AssetStatusAfterWork = x.AssetStatusAfterWork,
                    ClosedAt = x.ClosedAt,
                    DueDate = x.DueDate,
                    CreatedOn = x.CreatedOn,
                    WorkPerformed = x.WorkPerformed,
                    FinalResult = x.FinalResult,
                    Remarks = x.Remarks,
                    RejectionReason = x.RejectionReason,
                    CancellationReason = x.CancellationReason,
                    ApprovalRemarks = x.ApprovalRemarks,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? (x.ApprovedByUser.FullName ?? x.ApprovedByUser.UserName) : null,
                    ApprovedAt = x.ApprovedAt,
                    Diagnosis = x.Diagnosis == null ? null : new WorkOrderDiagnosisDto
                    {
                        Id = x.Diagnosis.Id,
                        InitialProblem = x.Diagnosis.InitialProblem,
                        Diagnosis = x.Diagnosis.Diagnosis,
                        RootCause = x.Diagnosis.RootCause,
                        ActionTaken = x.Diagnosis.ActionTaken,
                        FinalResult = x.Diagnosis.FinalResult,
                        DiagnosedByUserId = x.Diagnosis.DiagnosedByUserId,
                        DiagnosedByUserName = x.Diagnosis.DiagnosedByUser != null
                            ? (x.Diagnosis.DiagnosedByUser.FullName ?? x.Diagnosis.DiagnosedByUser.UserName) : null,
                        DiagnosedAt = x.Diagnosis.DiagnosedAt,
                        Remarks = x.Diagnosis.Remarks
                    },
                    StatusHistory = x.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new WorkOrderStatusHistoryDto
                    {
                        Id = h.Id,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        ChangedByUserId = h.ChangedByUserId,
                        ChangedByUserName = h.ChangedByUser.FullName ?? h.ChangedByUser.UserName,
                        ChangedAt = h.ChangedAt,
                        Remarks = h.Remarks
                    }).ToList(),
                    AssignmentHistory = x.AssignmentHistory.OrderBy(h => h.AssignedAt).Select(h => new WorkOrderAssignmentHistoryDto
                    {
                        Id = h.Id,
                        AssignedToUserId = h.AssignedToUserId,
                        AssignedToUserName = h.AssignedToUser.FullName ?? h.AssignedToUser.UserName,
                        AssignedByUserId = h.AssignedByUserId,
                        AssignedByUserName = h.AssignedByUser.FullName ?? h.AssignedByUser.UserName,
                        AssignedAt = h.AssignedAt,
                        UnassignedAt = h.UnassignedAt,
                        Remarks = h.Remarks
                    }).ToList(),
                    Attachments = x.Attachments.Where(a => !a.IsDeleted && a.IsActive).Select(a => new WorkOrderAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSizeBytes = a.FileSizeBytes,
                        DownloadUrl = $"/api/workorder/attachments/{a.Id}/download"
                    }).ToList()
                }).FirstOrDefaultAsync();
        }
    }
}
