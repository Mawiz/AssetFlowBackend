using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Workflow;
using AssetFlow.Data.Identity;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Workflow;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;
using static AssetFlow.Common.Enum.Enums;

namespace AssetFlow.Services.Core.Workflow
{
    public class WorkflowRuntimeService : IWorkflowRuntimeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowRuntimeService(ApplicationDbContext context, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Ok, string? Error)> InitiateIfMappedAsync(int? tenantId, string referenceItemType, int referenceItemId, int submitByUserId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.WorkflowMasters.AnyAsync(x =>
                x.TenantId == tenantId &&
                x.ReferenceItemType == referenceItemType &&
                x.ReferenceItemId == referenceItemId &&
                !x.IsDeleted, cancellationToken);
            if (exists) return (true, null);

            var mapping = await _context.WorkflowModuleItemMappings.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tenantId &&
                    x.ReferenceItemType == referenceItemType &&
                    x.IsActive &&
                    !x.IsDeleted, cancellationToken);
            if (mapping == null) return (true, null);

            var module = await _context.WorkflowModules.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == mapping.ModuleId && !m.IsDeleted && m.IsActive, cancellationToken);
            if (module == null || module.TenantId != tenantId)
                return (false, "Workflow module mapping points to an invalid module.");

            var initialStages = await _context.WorkflowModuleStages.AsNoTracking()
                .Where(s => s.ModuleId == mapping.ModuleId && s.IsInitialStage && s.IsActive && !s.IsDeleted)
                .ToListAsync(cancellationToken);
            if (initialStages.Count != 1)
                return (false, "Workflow module must have exactly one active initial stage (IsInitialStage).");

            var initialStage = initialStages[0];
            var initActions = await _context.WorkflowStageActions.AsNoTracking()
                .Where(a => a.StageId == initialStage.StageId && a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrderNo)
                .ToListAsync(cancellationToken);
            if (initActions.Count != 1)
                return (false, "Initial stage must have exactly one active stage action for automatic initiation.");
            var initAction = initActions[0];
            if (!initAction.NextSageId.HasValue)
                return (false, "Initial stage action must define NextSageId for the first transition.");

            var nextStage = await _context.WorkflowModuleStages.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StageId == initAction.NextSageId.Value && !s.IsDeleted && s.IsActive, cancellationToken);
            if (nextStage == null || nextStage.ModuleId != mapping.ModuleId)
                return (false, "Initial stage action next stage is invalid for this module.");

            var initialAssign = ResolveStageAssignment(initialStage);
            if (!initialAssign.Ok) return (false, initialAssign.Error);
            var nextAssign = ResolveStageAssignment(nextStage);
            if (!nextAssign.Ok) return (false, nextAssign.Error);

            var now = DateTime.UtcNow;
            var master = new WorkflowMaster
            {
                WorkflowMasterId = Guid.NewGuid(),
                ModuleId = mapping.ModuleId,
                ReferenceItemId = referenceItemId,
                ReferenceItemType = referenceItemType,
                SubmitBy = submitByUserId,
                CurrentStageId = nextStage.StageId,
                IsCompleted = false,
                TenantId = tenantId,
                CreatedById = submitByUserId
            };
            _context.WorkflowMasters.Add(master);

            var initialTask = BuildTask(master.WorkflowMasterId, initialStage, initialAssign.AssigneeTypeId, initialAssign.AssignmentValue, now, submitByUserId);
            initialTask.IsCompleted = true;
            initialTask.CompletedAt = now;
            initialTask.PerformedByUserId = submitByUserId;
            initialTask.StageActionId = initAction.StageActionId;
            initialTask.ActionId = initAction.ActionId;
            initialTask.Notes = "Workflow initiated.";
            _context.WorkflowTasks.Add(initialTask);

            var nextTask = BuildTask(master.WorkflowMasterId, nextStage, nextAssign.AssigneeTypeId, nextAssign.AssignmentValue, now, submitByUserId);
            _context.WorkflowTasks.Add(nextTask);

            await _context.SaveChangesAsync(cancellationToken);
            return (true, null);
        }

        public async Task<ResponseDto<WorkflowMasterDto>> GetMasterByIdAsync(Guid workflowMasterId)
        {
            var response = new ResponseDto<WorkflowMasterDto>();
            var dto = await MapMasterAsync(workflowMasterId);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<WorkflowMasterDto>> GetMasterByReferenceAsync(string referenceItemType, int referenceItemId)
        {
            var response = new ResponseDto<WorkflowMasterDto>();
            var tenantId = TenantScopeHelper.GetContextTenantId(_tenantProvider);
            var master = await _context.WorkflowMasters.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ReferenceItemType == referenceItemType &&
                    x.ReferenceItemId == referenceItemId &&
                    !x.IsDeleted &&
                    (!tenantId.HasValue || x.TenantId == tenantId));
            if (master == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var dto = await MapMasterAsync(master.WorkflowMasterId);
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<WorkflowTaskDetailDto>> GetTaskDetailAsync(Guid workflowTaskId)
        {
            var response = new ResponseDto<WorkflowTaskDetailDto>();
            var task = await LoadTaskWithAccessAsync(workflowTaskId);
            if (task == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var master = await MapMasterAsync(task.WorkflowMasterId);
            var history = await GetHistoryInternalAsync(task.WorkflowMasterId);
            var available = await GetAvailableActionsInternalAsync(task);

            response.Result = new WorkflowTaskDetailDto
            {
                WorkflowTaskId = task.WorkflowTaskId,
                WorkflowMasterId = task.WorkflowMasterId,
                ModuleStageId = task.ModuleStageId,
                StageNameEn = task.ModuleStage?.StageNameEn,
                StageActionId = task.StageActionId,
                ActionId = task.ActionId,
                AssigneeTypeId = task.AssigneeTypeId,
                AssignmentValue = task.AssignmentValue,
                AssigneeDisplayName = await ResolveAssigneeDisplayAsync(task.AssigneeTypeId, task.AssignmentValue, master?.TenantId),
                IsCompleted = task.IsCompleted,
                AssignedAt = task.AssignedAt,
                CompletedAt = task.CompletedAt,
                PerformedByUserId = task.PerformedByUserId,
                PerformedByDisplayName = await ResolveUserDisplayAsync(task.PerformedByUserId),
                PerformedByRoleId = task.PerformedByRoleId,
                Notes = task.Notes,
                DueDate = task.DueDate,
                DueInHours = task.DueInHours,
                ReferenceItemId = master?.ReferenceItemId ?? 0,
                ReferenceItemType = master?.ReferenceItemType ?? string.Empty,
                ModuleNameEn = master?.ModuleNameEn,
                Master = master,
                History = history,
                AvailableActions = available
            };
            return response;
        }

        public async Task<ResponseDto<List<WorkflowStageActionOptionDto>>> GetAvailableActionsAsync(Guid workflowTaskId)
        {
            var response = new ResponseDto<List<WorkflowStageActionOptionDto>>();
            var task = await LoadTaskWithAccessAsync(workflowTaskId);
            if (task == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (task.IsCompleted) { response.Result = new List<WorkflowStageActionOptionDto>(); return response; }
            response.Result = await GetAvailableActionsInternalAsync(task);
            return response;
        }

        public async Task<ResponseDto<WorkflowMasterDto>> PerformActionAsync(PerformWorkflowActionDto dto)
        {
            var response = new ResponseDto<WorkflowMasterDto>();
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) { response.AddError("User context is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var roleIds = UserHelper.GetCurrentRoleIds(_httpContextAccessor);
            var primaryRoleId = roleIds.FirstOrDefault();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = await _context.WorkflowTasks
                    .Include(t => t.ModuleStage)
                    .FirstOrDefaultAsync(t => t.WorkflowTaskId == dto.WorkflowTaskId && !t.IsDeleted);
                if (task == null || task.IsCompleted)
                {
                    response.AddError("Task not found or already completed.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var master = await _context.WorkflowMasters.FirstOrDefaultAsync(m => m.WorkflowMasterId == task.WorkflowMasterId && !m.IsDeleted);
                if (master == null || master.IsCompleted)
                {
                    response.AddError("Workflow is not active.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, master.TenantId);
                if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

                var active = await GetActiveTaskTrackedAsync(master.WorkflowMasterId);
                if (active == null || active.WorkflowTaskId != task.WorkflowTaskId)
                {
                    response.AddError("Only the current active task can be actioned.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var stageAction = await _context.WorkflowStageActions.AsNoTracking()
                    .Include(a => a.Action)
                    .FirstOrDefaultAsync(a => a.StageActionId == dto.StageActionId && !a.IsDeleted && a.IsActive);
                if (stageAction == null || stageAction.StageId != task.ModuleStageId)
                {
                    response.AddError("Invalid stage action for the current task.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var stage = task.ModuleStage ?? await _context.WorkflowModuleStages.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StageId == task.ModuleStageId);
                if (stage == null)
                {
                    response.AddError("Stage configuration not found.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (!CanUserPerformAction(stageAction, stage, userId.Value, roleIds))
                {
                    response.AddError("You are not authorized to perform this action.");
                    response.StatusCode = HttpStatusCode.Forbidden;
                    return response;
                }

                if (stageAction.Action?.IsNotesRequired == true && string.IsNullOrWhiteSpace(dto.Notes))
                {
                    response.AddError("Notes are required for this action.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var now = DateTime.UtcNow;
                task.IsCompleted = true;
                task.CompletedAt = now;
                task.PerformedByUserId = userId;
                task.PerformedByRoleId = primaryRoleId > 0 ? primaryRoleId : null;
                task.StageActionId = stageAction.StageActionId;
                task.ActionId = stageAction.ActionId;
                task.Notes = dto.Notes?.Trim();
                task.ModifiedOn = now;
                task.ModifiedById = userId;

                if (!stageAction.NextSageId.HasValue)
                {
                    if (!stage.IsClosingStage)
                    {
                        response.AddError("Stage action has no next stage and current stage is not configured as closing.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    master.IsCompleted = true;
                    master.CurrentStageId = stage.StageId;
                }
                else
                {
                    var nextStage = await _context.WorkflowModuleStages.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.StageId == stageAction.NextSageId.Value && !s.IsDeleted && s.IsActive);
                    if (nextStage == null || nextStage.ModuleId != master.ModuleId)
                    {
                        response.AddError("Next stage configuration is invalid.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }

                    var nextAssign = ResolveStageAssignment(nextStage);
                    if (!nextAssign.Ok)
                    {
                        response.AddError(nextAssign.Error!);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }

                    master.CurrentStageId = nextStage.StageId;
                    var nextTask = BuildTask(master.WorkflowMasterId, nextStage, nextAssign.AssigneeTypeId, nextAssign.AssignmentValue, now, userId);
                    _context.WorkflowTasks.Add(nextTask);
                }

                master.ModifiedOn = now;
                master.ModifiedById = userId;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.Result = await MapMasterAsync(master.WorkflowMasterId);
                return response;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ResponseDto<List<WorkflowTaskDto>>> GetHistoryAsync(Guid workflowMasterId)
        {
            var response = new ResponseDto<List<WorkflowTaskDto>>();
            var master = await _context.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(m => m.WorkflowMasterId == workflowMasterId && !m.IsDeleted);
            if (master == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, master.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = await GetHistoryInternalAsync(workflowMasterId);
            return response;
        }

        public async Task<ResponseDto<object>> FilterInboxAsync(WorkflowInboxFilterDto model)
        {
            var response = new ResponseDto<object>();
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) { response.AddError("User context is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var roleIds = UserHelper.GetCurrentRoleIds(_httpContextAccessor);
            var tenantId = TenantScopeHelper.GetContextTenantId(_tenantProvider);

            var category = (WorkflowInboxCategory)model.Category;
            var query = _context.WorkflowTasks.AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Join(_context.WorkflowMasters.Where(m => !m.IsDeleted),
                    t => t.WorkflowMasterId,
                    m => m.WorkflowMasterId,
                    (t, m) => new { Task = t, Master = m });

            if (tenantId.HasValue)
                query = query.Where(x => x.Master.TenantId == tenantId);

            query = category switch
            {
                WorkflowInboxCategory.Created => query.Where(x => x.Master.SubmitBy == userId.Value),
                WorkflowInboxCategory.PendingWithMe => query.Where(x =>
                    !x.Task.IsCompleted &&
                    !x.Master.IsCompleted &&
                    x.Task.WorkflowTaskId == _context.WorkflowTasks
                        .Where(t2 => t2.WorkflowMasterId == x.Master.WorkflowMasterId && !t2.IsDeleted && !t2.IsCompleted)
                        .OrderBy(t2 => t2.AssignedAt)
                        .Select(t2 => t2.WorkflowTaskId)
                        .FirstOrDefault() &&
                    (
                        (x.Task.AssigneeTypeId == (int)WorkflowPartyType.User && x.Task.AssignmentValue == userId.Value) ||
                        (x.Task.AssigneeTypeId == (int)WorkflowPartyType.Role && x.Task.AssignmentValue.HasValue && roleIds.Contains(x.Task.AssignmentValue.Value))
                    )),
                WorkflowInboxCategory.View => query.Where(x =>
                    !x.Master.IsCompleted &&
                    x.Task.WorkflowTaskId == _context.WorkflowTasks
                        .Where(t2 => t2.WorkflowMasterId == x.Master.WorkflowMasterId && !t2.IsDeleted && !t2.IsCompleted)
                        .OrderBy(t2 => t2.AssignedAt)
                        .Select(t2 => t2.WorkflowTaskId)
                        .FirstOrDefault() &&
                    _context.WorkflowStageActionViewers.Any(v =>
                        v.StageId == x.Task.ModuleStageId &&
                        v.IsActive &&
                        !v.IsDeleted &&
                        (
                            (v.ViewerTypeId == (int)WorkflowPartyType.User && v.ViewerValue == userId.Value) ||
                            (v.ViewerTypeId == (int)WorkflowPartyType.Role && roleIds.Contains(v.ViewerValue))
                        ))),
                WorkflowInboxCategory.PerformedByMe => query.Where(x => x.Task.IsCompleted && x.Task.PerformedByUserId == userId.Value),
                _ => query.Where(x => false)
            };

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x =>
                    x.Master.ReferenceItemType.Contains(s) ||
                    x.Master.ReferenceItemId.ToString() == s);
            }

            var projected = query.Select(x => x.Task);
            var orderProp = string.IsNullOrWhiteSpace(model.OrderByProp) ? "AssignedAt" : model.OrderByProp;
            var descending = model.SortDirection != (int)OrderBy.Ascending;
            var order = descending ? $"{orderProp} descending" : orderProp;

            var paged = await projected.OrderBy(order).ToPagedListAsync(model.PageNumber, model.PageSize);
            var items = new List<WorkflowTaskDto>();
            foreach (var t in paged)
                items.Add(await MapTaskAsync(t.WorkflowTaskId));

            response.Result = new { items, totalCount = paged.TotalItemCount };
            return response;
        }

        private async Task<WorkflowTask?> LoadTaskWithAccessAsync(Guid workflowTaskId)
        {
            var task = await _context.WorkflowTasks.AsNoTracking()
                .Include(t => t.ModuleStage)
                .FirstOrDefaultAsync(t => t.WorkflowTaskId == workflowTaskId && !t.IsDeleted);
            if (task == null) return null;
            var master = await _context.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(m => m.WorkflowMasterId == task.WorkflowMasterId);
            if (master == null) return null;
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, master.TenantId);
            if (!access.Ok) return null;
            return task;
        }

        private async Task<List<WorkflowStageActionOptionDto>> GetAvailableActionsInternalAsync(WorkflowTask task)
        {
            if (task.IsCompleted) return new List<WorkflowStageActionOptionDto>();
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) return new List<WorkflowStageActionOptionDto>();
            var roleIds = UserHelper.GetCurrentRoleIds(_httpContextAccessor);

            var active = await _context.WorkflowTasks.AsNoTracking()
                .Where(t => t.WorkflowMasterId == task.WorkflowMasterId && !t.IsDeleted && !t.IsCompleted)
                .OrderBy(t => t.AssignedAt)
                .Select(t => t.WorkflowTaskId)
                .FirstOrDefaultAsync();
            if (active != task.WorkflowTaskId) return new List<WorkflowStageActionOptionDto>();

            var stage = task.ModuleStage ?? await _context.WorkflowModuleStages.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StageId == task.ModuleStageId);
            if (stage == null) return new List<WorkflowStageActionOptionDto>();

            var actions = await _context.WorkflowStageActions.AsNoTracking()
                .Include(a => a.Action)
                .Where(a => a.StageId == task.ModuleStageId && a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrderNo)
                .ToListAsync();

            return actions
                .Where(a => CanUserPerformAction(a, stage, userId.Value, roleIds))
                .Select(a => new WorkflowStageActionOptionDto
                {
                    StageActionId = a.StageActionId,
                    ActionId = a.ActionId,
                    StageActionDisplayName = a.StageActionDisplayName,
                    IsNotesRequired = a.Action?.IsNotesRequired ?? false,
                    NextSageId = a.NextSageId
                })
                .ToList();
        }

        private static bool CanUserPerformAction(WorkflowStageAction stageAction, WorkflowModuleStage stage, int userId, IReadOnlyList<int> roleIds)
        {
            if (stageAction.AssigneeTypeId.HasValue && stageAction.AssignmentValue.HasValue)
                return MatchesParty(stageAction.AssigneeTypeId.Value, stageAction.AssignmentValue.Value, userId, roleIds);
            if (stage.AssigneeTypeId.HasValue)
            {
                if (stage.AssigneeTypeId == (int)WorkflowPartyType.User)
                    return false;
                if (stage.AssignmentValue.HasValue)
                    return MatchesParty(stage.AssigneeTypeId.Value, stage.AssignmentValue.Value, userId, roleIds);
            }
            return false;
        }

        private static bool MatchesParty(int assigneeTypeId, int assignmentValue, int userId, IReadOnlyList<int> roleIds) =>
            assigneeTypeId == (int)WorkflowPartyType.User
                ? assignmentValue == userId
                : assigneeTypeId == (int)WorkflowPartyType.Role && roleIds.Contains(assignmentValue);

        private static (bool Ok, string? Error, int? AssigneeTypeId, int? AssignmentValue) ResolveStageAssignment(WorkflowModuleStage stage)
        {
            if (!stage.AssigneeTypeId.HasValue)
                return (false, $"Stage '{stage.StageNameEn}' has no assignee configured.", null, null);
            if (stage.AssigneeTypeId == (int)WorkflowPartyType.User)
                return (false, $"Stage '{stage.StageNameEn}' uses runtime user assignment; configure a role or specific user for automatic task assignment.", null, null);
            if (!stage.AssignmentValue.HasValue)
                return (false, $"Stage '{stage.StageNameEn}' requires a role assignment value.", null, null);
            return (true, null, stage.AssigneeTypeId, stage.AssignmentValue);
        }

        private WorkflowTask BuildTask(Guid masterId, WorkflowModuleStage stage, int? assigneeTypeId, int? assignmentValue, DateTime assignedAt, int? createdByUserId)
        {
            DateTime? dueDate = null;
            if (stage.DueInHours > 0)
                dueDate = assignedAt.AddHours(stage.DueInHours);

            return new WorkflowTask
            {
                WorkflowTaskId = Guid.NewGuid(),
                WorkflowMasterId = masterId,
                ModuleStageId = stage.StageId,
                AssigneeTypeId = assigneeTypeId,
                AssignmentValue = assignmentValue,
                IsCompleted = false,
                AssignedAt = assignedAt,
                DueDate = dueDate,
                DueInHours = stage.DueInHours,
                CreatedById = createdByUserId,
                IsActive = true
            };
        }

        private async Task<WorkflowTask?> GetActiveTaskTrackedAsync(Guid masterId) =>
            await _context.WorkflowTasks
                .Where(t => t.WorkflowMasterId == masterId && !t.IsDeleted && !t.IsCompleted)
                .OrderBy(t => t.AssignedAt)
                .FirstOrDefaultAsync();

        private async Task<WorkflowMasterDto?> MapMasterAsync(Guid workflowMasterId)
        {
            var master = await _context.WorkflowMasters.AsNoTracking()
                .FirstOrDefaultAsync(m => m.WorkflowMasterId == workflowMasterId && !m.IsDeleted);
            if (master == null) return null;
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, master.TenantId);
            if (!access.Ok) return null;

            var module = await _context.WorkflowModules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == master.ModuleId);
            var submitter = await ResolveUserDisplayAsync(master.SubmitBy);
            var activeTask = await _context.WorkflowTasks.AsNoTracking()
                .Where(t => t.WorkflowMasterId == master.WorkflowMasterId && !t.IsDeleted && !t.IsCompleted)
                .OrderBy(t => t.AssignedAt)
                .FirstOrDefaultAsync();

            string? stageName = null;
            string? pendingAssignee = null;
            if (activeTask != null)
            {
                stageName = await _context.WorkflowModuleStages.Where(s => s.StageId == activeTask.ModuleStageId)
                    .Select(s => s.StageNameEn).FirstOrDefaultAsync();
                pendingAssignee = await ResolveAssigneeDisplayAsync(activeTask.AssigneeTypeId, activeTask.AssignmentValue, master.TenantId);
            }
            else if (master.CurrentStageId.HasValue)
            {
                stageName = await _context.WorkflowModuleStages.Where(s => s.StageId == master.CurrentStageId)
                    .Select(s => s.StageNameEn).FirstOrDefaultAsync();
            }

            var lastCompleted = await _context.WorkflowTasks.AsNoTracking()
                .Where(t => t.WorkflowMasterId == master.WorkflowMasterId && t.IsCompleted && !t.IsDeleted)
                .OrderByDescending(t => t.CompletedAt)
                .FirstOrDefaultAsync();
            string? lastOp = null;
            if (lastCompleted != null)
            {
                var actionName = lastCompleted.ActionId.HasValue
                    ? await _context.WorkflowActionDefinitions.Where(a => a.ActionId == lastCompleted.ActionId).Select(a => a.AfterActionEn).FirstOrDefaultAsync()
                    : null;
                var performer = await ResolveUserDisplayAsync(lastCompleted.PerformedByUserId);
                lastOp = $"{actionName ?? "Action"} by {performer ?? "—"}";
            }

            return new WorkflowMasterDto
            {
                WorkflowMasterId = master.WorkflowMasterId,
                ModuleId = master.ModuleId,
                ModuleNameEn = module?.ModuleNameEn,
                ReferenceItemId = master.ReferenceItemId,
                ReferenceItemType = master.ReferenceItemType,
                SubmitBy = master.SubmitBy,
                SubmitByDisplayName = submitter,
                CurrentStageId = master.CurrentStageId,
                CurrentStageName = stageName,
                IsCompleted = master.IsCompleted,
                TenantId = master.TenantId,
                ActiveWorkflowTaskId = activeTask?.WorkflowTaskId,
                PendingAssigneeDisplayName = pendingAssignee,
                LastOperationDisplay = lastOp
            };
        }

        private async Task<List<WorkflowTaskDto>> GetHistoryInternalAsync(Guid workflowMasterId)
        {
            var master = await _context.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(m => m.WorkflowMasterId == workflowMasterId);
            var tasks = await _context.WorkflowTasks.AsNoTracking()
                .Where(t => t.WorkflowMasterId == workflowMasterId && !t.IsDeleted)
                .OrderBy(t => t.AssignedAt)
                .ToListAsync();
            var list = new List<WorkflowTaskDto>();
            foreach (var t in tasks)
                list.Add(await MapTaskAsync(t.WorkflowTaskId, master));
            return list;
        }

        private async Task<WorkflowTaskDto> MapTaskAsync(Guid taskId, WorkflowMaster? master = null)
        {
            master ??= await _context.WorkflowMasters.AsNoTracking()
                .FirstOrDefaultAsync(m => m.WorkflowMasterId ==
                    _context.WorkflowTasks.Where(t => t.WorkflowTaskId == taskId).Select(t => t.WorkflowMasterId).First());
            var task = await _context.WorkflowTasks.AsNoTracking()
                .Include(t => t.ModuleStage)
                .FirstAsync(t => t.WorkflowTaskId == taskId);
            var actionName = task.ActionId.HasValue
                ? await _context.WorkflowActionDefinitions.Where(a => a.ActionId == task.ActionId).Select(a => a.AfterActionEn).FirstOrDefaultAsync()
                : null;
            return new WorkflowTaskDto
            {
                WorkflowTaskId = task.WorkflowTaskId,
                WorkflowMasterId = task.WorkflowMasterId,
                ModuleStageId = task.ModuleStageId,
                StageNameEn = task.ModuleStage?.StageNameEn,
                StageActionId = task.StageActionId,
                ActionId = task.ActionId,
                ActionDisplayName = actionName,
                AssigneeTypeId = task.AssigneeTypeId,
                AssignmentValue = task.AssignmentValue,
                AssigneeDisplayName = await ResolveAssigneeDisplayAsync(task.AssigneeTypeId, task.AssignmentValue, master?.TenantId),
                IsCompleted = task.IsCompleted,
                AssignedAt = task.AssignedAt,
                CompletedAt = task.CompletedAt,
                PerformedByUserId = task.PerformedByUserId,
                PerformedByDisplayName = await ResolveUserDisplayAsync(task.PerformedByUserId),
                PerformedByRoleId = task.PerformedByRoleId,
                Notes = task.Notes,
                DueDate = task.DueDate,
                DueInHours = task.DueInHours,
                ReferenceItemId = master?.ReferenceItemId ?? 0,
                ReferenceItemType = master?.ReferenceItemType ?? string.Empty,
                ModuleNameEn = master == null ? null : await _context.WorkflowModules.Where(m => m.ModuleId == master.ModuleId).Select(m => m.ModuleNameEn).FirstOrDefaultAsync()
            };
        }

        private async Task<string?> ResolveUserDisplayAsync(int? userId)
        {
            if (!userId.HasValue) return null;
            return await _context.ApplicationUsers.Where(u => u.Id == userId.Value)
                .Select(u => u.FullName ?? u.UserName).FirstOrDefaultAsync();
        }

        private async Task<string?> ResolveAssigneeDisplayAsync(int? typeId, int? value, int? tenantId)
        {
            if (!typeId.HasValue || !value.HasValue) return null;
            if (typeId == (int)WorkflowPartyType.User)
                return await ResolveUserDisplayAsync(value);
            if (typeId == (int)WorkflowPartyType.Role)
                return await _context.ApplicationRoles.Where(r => r.Id == value.Value && (!tenantId.HasValue || r.TenantId == tenantId))
                    .Select(r => r.DisplayName ?? r.Name).FirstOrDefaultAsync();
            return null;
        }
    }
}
