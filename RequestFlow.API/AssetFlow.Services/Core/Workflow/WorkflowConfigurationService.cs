using AssetFlow.Common.Enum;

using AssetFlow.Data.Data;

using AssetFlow.Data.Entities.Workflow;

using AssetFlow.Data.Identity;

using AssetFlow.Data.Provider;

using AssetFlow.Services.Contracts;

using AssetFlow.Services.Dto;

using AssetFlow.Services.Dto.Workflow;

using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core;

using System.Net;

using X.PagedList;

using static AssetFlow.Common.Enum.Enums;



namespace AssetFlow.Services.Core.Workflow

{

    public class WorkflowConfigurationService : IWorkflowConfigurationService

    {

        private readonly ApplicationDbContext _context;

        private readonly ITenantProvider _tenantProvider;



        public WorkflowConfigurationService(ApplicationDbContext context, ITenantProvider tenantProvider)

        {

            _context = context;

            _tenantProvider = tenantProvider;

        }



        #region Modules



        public async Task<ResponseDto<WorkflowModuleDto>> CreateModuleAsync(CreateWorkflowModuleDto dto)

        {

            var response = new ResponseDto<WorkflowModuleDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);

            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (string.IsNullOrWhiteSpace(dto.ModuleNameEn))

            { response.AddError("Module name (English) is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var entity = new WorkflowModule

            {

                ModuleId = await WorkflowIdAllocator.NextModuleIdAsync(_context),

                ModuleNameEn = dto.ModuleNameEn.Trim(),

                ModuleNameAr = dto.ModuleNameAr?.Trim(),

                DisplayOrder = dto.DisplayOrder,

                IsReOpen = dto.IsReOpen,

                TenantId = tenantResult.TenantId,

                IsActive = dto.IsActive

            };

            _context.WorkflowModules.Add(entity);

            await _context.SaveChangesAsync();

            response.Result = await MapModuleAsync(entity.ModuleId);

            return response;

        }



        public async Task<ResponseDto<WorkflowModuleDto>> UpdateModuleAsync(UpdateWorkflowModuleDto dto)

        {

            var response = new ResponseDto<WorkflowModuleDto>();

            var entity = await FindModuleTrackedAsync(dto.ModuleId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }



            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);

            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (string.IsNullOrWhiteSpace(dto.ModuleNameEn))

            { response.AddError("Module name (English) is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            entity.ModuleNameEn = dto.ModuleNameEn.Trim();

            entity.ModuleNameAr = dto.ModuleNameAr?.Trim();

            entity.DisplayOrder = dto.DisplayOrder;

            entity.IsReOpen = dto.IsReOpen;

            entity.TenantId = tenantResult.TenantId;

            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            response.Result = await MapModuleAsync(entity.ModuleId);

            return response;

        }



        public async Task<ResponseDto<WorkflowModuleDto>> GetModuleByIdAsync(int moduleId)

        {

            var response = new ResponseDto<WorkflowModuleDto>();

            var dto = await MapModuleAsync(moduleId);

            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            response.Result = dto;

            return response;

        }



        public async Task<ResponseDto<List<WorkflowModuleDto>>> GetAllModulesAsync(int? tenantId)

        {

            var response = new ResponseDto<List<WorkflowModuleDto>>();

            var query = TenantScopeHelper.ApplyTenantScope(_context.WorkflowModules.AsQueryable(), _tenantProvider, tenantId)

                .Where(x => !x.IsDeleted && x.IsActive);

            response.Result = await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.ModuleNameEn)

                .Select(x => new WorkflowModuleDto

                {

                    ModuleId = x.ModuleId,

                    ModuleNameEn = x.ModuleNameEn,

                    ModuleNameAr = x.ModuleNameAr,

                    DisplayOrder = x.DisplayOrder,

                    IsReOpen = x.IsReOpen,

                    TenantId = x.TenantId,

                    TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,

                    IsActive = x.IsActive

                }).ToListAsync();

            return response;

        }



        public async Task<ResponseDto<object>> FilterModulesAsync(WorkflowModuleFilterDto model)

        {

            var response = new ResponseDto<object>();

            var query = TenantScopeHelper.ApplyTenantScope(_context.WorkflowModules.AsQueryable(), _tenantProvider, model.TenantId)

                .Where(x => !x.IsDeleted);

            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);

            if (!string.IsNullOrWhiteSpace(model.SearchText))

            {

                var s = model.SearchText.Trim();

                query = query.Where(x => x.ModuleNameEn.Contains(s) || (x.ModuleNameAr != null && x.ModuleNameAr.Contains(s)));

            }

            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "DisplayOrder" : model.OrderByProp;

            if (model.SortDirection == (int)OrderBy.Descending) order += " descending";

            var page = await query.OrderBy(order).Select(x => new WorkflowModuleDto

            {

                ModuleId = x.ModuleId,

                ModuleNameEn = x.ModuleNameEn,

                ModuleNameAr = x.ModuleNameAr,

                DisplayOrder = x.DisplayOrder,

                IsReOpen = x.IsReOpen,

                TenantId = x.TenantId,

                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,

                IsActive = x.IsActive

            }).ToPagedListAsync(model.PageNumber, model.PageSize);

            response.Result = page;

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteModuleAsync(int moduleId)

        {

            var response = new ResponseDto<bool>();

            var entity = await FindModuleTrackedAsync(moduleId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #endregion



        #region Stages



        public async Task<ResponseDto<WorkflowModuleStageDto>> CreateStageAsync(CreateWorkflowModuleStageDto dto)

        {

            var response = new ResponseDto<WorkflowModuleStageDto>();

            var module = await FindModuleAsync(dto.ModuleId);

            if (module == null) { response.AddError("Module not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, module.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            if (string.IsNullOrWhiteSpace(dto.StageNameEn))

            { response.AddError("Stage name (English) is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var assigneeCheck = await ValidateStageAssigneeAsync(module.TenantId, dto.AssigneeTypeId, dto.AssignmentValue);

            if (!assigneeCheck.Ok) { response.AddError(assigneeCheck.Error!); response.StatusCode = assigneeCheck.StatusCode; return response; }



            var entity = new WorkflowModuleStage

            {

                StageId = await WorkflowIdAllocator.NextStageIdAsync(_context),

                ModuleId = dto.ModuleId,

                StageNameEn = dto.StageNameEn.Trim(),

                StageNameAr = dto.StageNameAr?.Trim(),

                SequenceNo = dto.SequenceNo,

                DueInHours = dto.DueInHours,

                VersionNo = dto.VersionNo,

                EmailTemplateId = dto.EmailTemplateId,

                IsFinalApprover = dto.IsFinalApprover,

                AcknowledgementTemplateId = dto.AcknowledgementTemplateId,

                IsClosingStage = dto.IsClosingStage,

                IsInitialStage = dto.IsInitialStage,

                IsLogOpen = dto.IsLogOpen,

                IsTATExtension = dto.IsTATExtension,

                AssigneeTypeId = dto.AssigneeTypeId,

                AssignmentValue = dto.AssigneeTypeId == (int)Enums.WorkflowPartyType.User ? null : dto.AssignmentValue,

                IsActive = dto.IsActive

            };

            _context.WorkflowModuleStages.Add(entity);

            if (dto.IsInitialStage)

                await ClearOtherInitialStagesAsync(dto.ModuleId, entity.StageId);

            await _context.SaveChangesAsync();

            response.Result = await MapStageAsync(entity.StageId);

            return response;

        }



        public async Task<ResponseDto<WorkflowModuleStageDto>> UpdateStageAsync(UpdateWorkflowModuleStageDto dto)

        {

            var response = new ResponseDto<WorkflowModuleStageDto>();

            var entity = await FindStageTrackedAsync(dto.StageId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var module = entity.Module ?? await FindModuleAsync(entity.ModuleId);

            if (module == null) { response.AddError("Module not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, module.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }



            if (dto.ModuleId != entity.ModuleId)

            {

                var newModule = await FindModuleAsync(dto.ModuleId);

                if (newModule == null) { response.AddError("Module not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

                var newAccess = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, newModule.TenantId);

                if (!newAccess.Ok) { response.AddError(newAccess.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            }

            if (string.IsNullOrWhiteSpace(dto.StageNameEn))

            { response.AddError("Stage name (English) is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var assigneeCheck = await ValidateStageAssigneeAsync(module.TenantId, dto.AssigneeTypeId, dto.AssignmentValue);

            if (!assigneeCheck.Ok) { response.AddError(assigneeCheck.Error!); response.StatusCode = assigneeCheck.StatusCode; return response; }



            entity.ModuleId = dto.ModuleId;

            entity.StageNameEn = dto.StageNameEn.Trim();

            entity.StageNameAr = dto.StageNameAr?.Trim();

            entity.SequenceNo = dto.SequenceNo;

            entity.DueInHours = dto.DueInHours;

            entity.VersionNo = dto.VersionNo;

            entity.EmailTemplateId = dto.EmailTemplateId;

            entity.IsFinalApprover = dto.IsFinalApprover;

            entity.AcknowledgementTemplateId = dto.AcknowledgementTemplateId;

            entity.IsClosingStage = dto.IsClosingStage;

            entity.IsInitialStage = dto.IsInitialStage;

            entity.IsLogOpen = dto.IsLogOpen;

            entity.IsTATExtension = dto.IsTATExtension;

            entity.AssigneeTypeId = dto.AssigneeTypeId;

            entity.AssignmentValue = dto.AssigneeTypeId == (int)Enums.WorkflowPartyType.User ? null : dto.AssignmentValue;

            entity.IsActive = dto.IsActive;

            if (dto.IsInitialStage)

                await ClearOtherInitialStagesAsync(entity.ModuleId, entity.StageId);

            await _context.SaveChangesAsync();

            response.Result = await MapStageAsync(entity.StageId);

            return response;

        }



        public async Task<ResponseDto<WorkflowModuleStageDto>> GetStageByIdAsync(int stageId)

        {

            var response = new ResponseDto<WorkflowModuleStageDto>();

            var stage = await _context.WorkflowModuleStages.AsNoTracking()

                .Include(s => s.Module)

                .FirstOrDefaultAsync(s => s.StageId == stageId && !s.IsDeleted);

            if (stage == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, stage.Module?.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            response.Result = await MapStageDtoAsync(stage, stage.Module?.TenantId);

            return response;

        }



        public async Task<ResponseDto<List<WorkflowModuleStageDto>>> GetAllStagesAsync(int moduleId)

        {

            var response = new ResponseDto<List<WorkflowModuleStageDto>>();

            var module = await FindModuleAsync(moduleId);

            if (module == null) { response.AddError("Module not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, module.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }



            response.Result = await _context.WorkflowModuleStages.AsNoTracking()

                .Where(s => s.ModuleId == moduleId && !s.IsDeleted && s.IsActive)

                .OrderBy(s => s.SequenceNo).ThenBy(s => s.StageNameEn)

                .Select(s => new WorkflowModuleStageDto

                {

                    StageId = s.StageId,

                    ModuleId = s.ModuleId,

                    StageNameEn = s.StageNameEn,

                    StageNameAr = s.StageNameAr,

                    SequenceNo = s.SequenceNo,

                    DueInHours = s.DueInHours,

                    VersionNo = s.VersionNo,

                    EmailTemplateId = s.EmailTemplateId,

                    IsFinalApprover = s.IsFinalApprover,

                    AcknowledgementTemplateId = s.AcknowledgementTemplateId,

                    IsClosingStage = s.IsClosingStage,

                    IsInitialStage = s.IsInitialStage,

                    IsLogOpen = s.IsLogOpen,

                    IsTATExtension = s.IsTATExtension,

                    AssigneeTypeId = s.AssigneeTypeId,

                    AssignmentValue = s.AssignmentValue,

                    IsActive = s.IsActive

                }).ToListAsync();

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteStageAsync(int stageId)

        {

            var response = new ResponseDto<bool>();

            var entity = await FindStageTrackedAsync(stageId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var module = entity.Module ?? await FindModuleAsync(entity.ModuleId);

            if (module == null) { response.AddError("Module not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, module.TenantId);

            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #endregion



        #region Actions



        public async Task<ResponseDto<WorkflowActionDto>> CreateActionAsync(CreateWorkflowActionDto dto)

        {

            var response = new ResponseDto<WorkflowActionDto>();

            if (string.IsNullOrWhiteSpace(dto.BeforeActionEn) || string.IsNullOrWhiteSpace(dto.AfterActionEn) || string.IsNullOrWhiteSpace(dto.BaseCssClass))

            { response.AddError("Before action, after action, and CSS class are required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var entity = new WorkflowActionDefinition

            {

                BeforeActionEn = dto.BeforeActionEn.Trim(),

                BeforeActionAr = dto.BeforeActionAr?.Trim() ?? string.Empty,

                AfterActionEn = dto.AfterActionEn.Trim(),

                AfterActionAr = dto.AfterActionAr?.Trim() ?? string.Empty,

                BaseCssClass = dto.BaseCssClass.Trim(),

                IsNotesRequired = dto.IsNotesRequired,

                IsActive = dto.IsActive

            };

            _context.WorkflowActionDefinitions.Add(entity);

            await _context.SaveChangesAsync();

            response.Result = MapActionEntity(entity);

            return response;

        }



        public async Task<ResponseDto<WorkflowActionDto>> UpdateActionAsync(UpdateWorkflowActionDto dto)

        {

            var response = new ResponseDto<WorkflowActionDto>();

            var entity = await FindActionTrackedAsync(dto.ActionId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            if (string.IsNullOrWhiteSpace(dto.BeforeActionEn) || string.IsNullOrWhiteSpace(dto.AfterActionEn) || string.IsNullOrWhiteSpace(dto.BaseCssClass))

            { response.AddError("Before action, after action, and CSS class are required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            entity.BeforeActionEn = dto.BeforeActionEn.Trim();

            entity.BeforeActionAr = dto.BeforeActionAr?.Trim() ?? string.Empty;

            entity.AfterActionEn = dto.AfterActionEn.Trim();

            entity.AfterActionAr = dto.AfterActionAr?.Trim() ?? string.Empty;

            entity.BaseCssClass = dto.BaseCssClass.Trim();

            entity.IsNotesRequired = dto.IsNotesRequired;

            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            response.Result = MapActionEntity(entity);

            return response;

        }



        public async Task<ResponseDto<WorkflowActionDto>> GetActionByIdAsync(int actionId)

        {

            var response = new ResponseDto<WorkflowActionDto>();

            var entity = await FindActionAsync(actionId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            response.Result = MapActionEntity(entity);

            return response;

        }



        public async Task<ResponseDto<List<WorkflowActionDto>>> GetAllActionsAsync()

        {

            var response = new ResponseDto<List<WorkflowActionDto>>();

            response.Result = await _context.WorkflowActionDefinitions.AsNoTracking()

                .Where(x => !x.IsDeleted && x.IsActive)

                .OrderBy(x => x.AfterActionEn)

                .Select(x => new WorkflowActionDto

                {

                    ActionId = x.ActionId,

                    BeforeActionEn = x.BeforeActionEn,

                    BeforeActionAr = x.BeforeActionAr,

                    AfterActionEn = x.AfterActionEn,

                    AfterActionAr = x.AfterActionAr,

                    BaseCssClass = x.BaseCssClass,

                    IsNotesRequired = x.IsNotesRequired,

                    IsActive = x.IsActive

                }).ToListAsync();

            return response;

        }



        public async Task<ResponseDto<object>> FilterActionsAsync(WorkflowActionFilterDto model)

        {

            var response = new ResponseDto<object>();

            var query = _context.WorkflowActionDefinitions.AsQueryable().Where(x => !x.IsDeleted);

            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);

            if (!string.IsNullOrWhiteSpace(model.SearchText))

            {

                var s = model.SearchText.Trim();

                query = query.Where(x => x.BeforeActionEn.Contains(s) || x.AfterActionEn.Contains(s)

                    || x.BeforeActionAr.Contains(s) || x.AfterActionAr.Contains(s) || x.BaseCssClass.Contains(s));

            }

            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "AfterActionEn" : model.OrderByProp;

            if (model.SortDirection == (int)OrderBy.Descending) order += " descending";

            var page = await query.OrderBy(order).Select(x => new WorkflowActionDto

            {

                ActionId = x.ActionId,

                BeforeActionEn = x.BeforeActionEn,

                BeforeActionAr = x.BeforeActionAr,

                AfterActionEn = x.AfterActionEn,

                AfterActionAr = x.AfterActionAr,

                BaseCssClass = x.BaseCssClass,

                IsNotesRequired = x.IsNotesRequired,

                IsActive = x.IsActive

            }).ToPagedListAsync(model.PageNumber, model.PageSize);

            response.Result = page;

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteActionAsync(int actionId)

        {

            var response = new ResponseDto<bool>();

            var entity = await FindActionTrackedAsync(actionId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #endregion



        #region Stage actions



        public async Task<ResponseDto<WorkflowStageActionDto>> CreateStageActionAsync(CreateWorkflowStageActionDto dto)

        {

            var response = new ResponseDto<WorkflowStageActionDto>();

            var stageCtx = await GetStageContextAsync(dto.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var validation = await ValidateStageActionReferencesAsync(dto.ActionId, dto.NextSageId, stageCtx.ModuleId, stageCtx.TenantId, dto.StageId,

                dto.AssigneeTypeId, dto.AssignmentValue);

            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }

            if (string.IsNullOrWhiteSpace(dto.StageActionDisplayName))

            { response.AddError("Stage action display name is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var entity = new WorkflowStageAction

            {

                StageActionId = await WorkflowIdAllocator.NextStageActionIdAsync(_context),

                StageId = dto.StageId,

                ActionId = dto.ActionId,

                StageActionDisplayName = dto.StageActionDisplayName.Trim(),

                StageActionDisplayNameAr = dto.StageActionDisplayNameAr?.Trim(),

                EmailTemplateId = dto.EmailTemplateId,

                DisplayOrderNo = dto.DisplayOrderNo,

                CssClass = dto.CssClass?.Trim(),

                NextSageId = dto.NextSageId,

                AssigneeTypeId = dto.AssigneeTypeId,

                AssignmentValue = dto.AssignmentValue,

                AcknowledgementTemplateId = dto.AcknowledgementTemplateId,

                ActionPopUpRenderingComponents = dto.ActionPopUpRenderingComponents,

                TemplateId = dto.TemplateId,

                SMSTemplateId = dto.SMSTemplateId,

                IsActive = dto.IsActive

            };

            _context.WorkflowStageActions.Add(entity);

            await _context.SaveChangesAsync();

            response.Result = await MapStageActionAsync(entity.StageActionId);

            return response;

        }



        public async Task<ResponseDto<WorkflowStageActionDto>> UpdateStageActionAsync(UpdateWorkflowStageActionDto dto)

        {

            var response = new ResponseDto<WorkflowStageActionDto>();

            var entity = await FindStageActionTrackedAsync(dto.StageActionId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var stageId = dto.StageId;

            var stageCtx = await GetStageContextAsync(stageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var validation = await ValidateStageActionReferencesAsync(dto.ActionId, dto.NextSageId, stageCtx.ModuleId, stageCtx.TenantId, stageId,

                dto.AssigneeTypeId, dto.AssignmentValue);

            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }

            if (string.IsNullOrWhiteSpace(dto.StageActionDisplayName))

            { response.AddError("Stage action display name is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.StageId = dto.StageId;

            entity.ActionId = dto.ActionId;

            entity.StageActionDisplayName = dto.StageActionDisplayName.Trim();

            entity.StageActionDisplayNameAr = dto.StageActionDisplayNameAr?.Trim();

            entity.EmailTemplateId = dto.EmailTemplateId;

            entity.DisplayOrderNo = dto.DisplayOrderNo;

            entity.CssClass = dto.CssClass?.Trim();

            entity.NextSageId = dto.NextSageId;

            entity.AssigneeTypeId = dto.AssigneeTypeId;

            entity.AssignmentValue = dto.AssignmentValue;

            entity.AcknowledgementTemplateId = dto.AcknowledgementTemplateId;

            entity.ActionPopUpRenderingComponents = dto.ActionPopUpRenderingComponents;

            entity.TemplateId = dto.TemplateId;

            entity.SMSTemplateId = dto.SMSTemplateId;

            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            response.Result = await MapStageActionAsync(entity.StageActionId);

            return response;

        }



        public async Task<ResponseDto<WorkflowStageActionDto>> GetStageActionByIdAsync(int stageActionId)

        {

            var response = new ResponseDto<WorkflowStageActionDto>();

            var dto = await MapStageActionAsync(stageActionId);

            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var stageCtx = await GetStageContextAsync(dto.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }

            response.Result = dto;

            return response;

        }



        public async Task<ResponseDto<List<WorkflowStageActionDto>>> GetAllStageActionsAsync(int stageId)

        {

            var response = new ResponseDto<List<WorkflowStageActionDto>>();

            var stageCtx = await GetStageContextAsync(stageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var items = await _context.WorkflowStageActions.AsNoTracking()

                .Include(a => a.Action)

                .Where(a => a.StageId == stageId && !a.IsDeleted && a.IsActive)

                .OrderBy(a => a.DisplayOrderNo)

                .ToListAsync();



            var nextStageIds = items.Where(a => a.NextSageId.HasValue).Select(a => a.NextSageId!.Value).Distinct().ToList();

            var nextStages = nextStageIds.Count == 0

                ? new Dictionary<int, string>()

                : await _context.WorkflowModuleStages.AsNoTracking()

                    .Where(s => nextStageIds.Contains(s.StageId) && !s.IsDeleted)

                    .ToDictionaryAsync(s => s.StageId, s => s.StageNameEn);



            var tenantId = stageCtx.TenantId;

            response.Result = new List<WorkflowStageActionDto>();

            foreach (var item in items)

            {

                response.Result.Add(await MapStageActionFromEntityAsync(item, nextStages, tenantId));

            }

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteStageActionAsync(int stageActionId)

        {

            var response = new ResponseDto<bool>();

            var entity = await FindStageActionTrackedAsync(stageActionId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var stageCtx = await GetStageContextAsync(entity.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #endregion



        #region Stage action viewers



        public async Task<ResponseDto<WorkflowStageActionViewerDto>> CreateStageActionViewerAsync(CreateWorkflowStageActionViewerDto dto)

        {

            var response = new ResponseDto<WorkflowStageActionViewerDto>();

            var stageCtx = await GetStageContextAsync(dto.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var validation = await ValidateViewerAsync(stageCtx.TenantId, dto.ViewerTypeId, dto.ViewerValue);

            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }



            var entity = new WorkflowStageActionViewer

            {

                StageId = dto.StageId,

                ViewerTypeId = dto.ViewerTypeId,

                ViewerValue = dto.ViewerValue,

                IsActive = dto.IsActive

            };

            _context.WorkflowStageActionViewers.Add(entity);

            await _context.SaveChangesAsync();

            response.Result = await MapViewerAsync(entity.StageActionViewerId, stageCtx.TenantId);

            return response;

        }



        public async Task<ResponseDto<WorkflowStageActionViewerDto>> UpdateStageActionViewerAsync(UpdateWorkflowStageActionViewerDto dto)

        {

            var response = new ResponseDto<WorkflowStageActionViewerDto>();

            var entity = await FindViewerTrackedAsync(dto.StageActionViewerId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var stageCtx = await GetStageContextAsync(dto.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var validation = await ValidateViewerAsync(stageCtx.TenantId, dto.ViewerTypeId, dto.ViewerValue);

            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }



            entity.StageId = dto.StageId;

            entity.ViewerTypeId = dto.ViewerTypeId;

            entity.ViewerValue = dto.ViewerValue;

            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            response.Result = await MapViewerAsync(entity.StageActionViewerId, stageCtx.TenantId);

            return response;

        }



        public async Task<ResponseDto<WorkflowStageActionViewerDto>> GetStageActionViewerByIdAsync(int stageActionViewerId)

        {

            var response = new ResponseDto<WorkflowStageActionViewerDto>();

            var entity = await FindViewerAsync(stageActionViewerId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var stageCtx = await GetStageContextAsync(entity.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }

            response.Result = await MapViewerAsync(entity.StageActionViewerId, stageCtx.TenantId);

            return response;

        }



        public async Task<ResponseDto<List<WorkflowStageActionViewerDto>>> GetAllStageActionViewersAsync(int stageId)

        {

            var response = new ResponseDto<List<WorkflowStageActionViewerDto>>();

            var stageCtx = await GetStageContextAsync(stageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }



            var items = await _context.WorkflowStageActionViewers.AsNoTracking()

                .Where(v => v.StageId == stageId && !v.IsDeleted && v.IsActive)

                .OrderBy(v => v.ViewerTypeId).ThenBy(v => v.ViewerValue)

                .ToListAsync();



            response.Result = new List<WorkflowStageActionViewerDto>();

            foreach (var item in items)

            {

                response.Result.Add(await MapViewerFromEntityAsync(item, stageCtx.TenantId));

            }

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteStageActionViewerAsync(int stageActionViewerId)

        {

            var response = new ResponseDto<bool>();

            var entity = await FindViewerTrackedAsync(stageActionViewerId);

            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var stageCtx = await GetStageContextAsync(entity.StageId);

            if (!stageCtx.Ok) { response.AddError(stageCtx.Error!); response.StatusCode = stageCtx.StatusCode; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #endregion



        #region Helpers



        private async Task<WorkflowModule?> FindModuleAsync(int moduleId) =>

            await _context.WorkflowModules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == moduleId && !m.IsDeleted);



        private async Task<WorkflowModule?> FindModuleTrackedAsync(int moduleId) =>

            await _context.WorkflowModules.FirstOrDefaultAsync(m => m.ModuleId == moduleId && !m.IsDeleted);



        private async Task<WorkflowModuleStage?> FindStageTrackedAsync(int stageId) =>

            await _context.WorkflowModuleStages.Include(s => s.Module).FirstOrDefaultAsync(s => s.StageId == stageId && !s.IsDeleted);



        private async Task<WorkflowActionDefinition?> FindActionAsync(int actionId) =>

            await _context.WorkflowActionDefinitions.AsNoTracking().FirstOrDefaultAsync(a => a.ActionId == actionId && !a.IsDeleted);



        private async Task<WorkflowActionDefinition?> FindActionTrackedAsync(int actionId) =>

            await _context.WorkflowActionDefinitions.FirstOrDefaultAsync(a => a.ActionId == actionId && !a.IsDeleted);



        private async Task<WorkflowStageAction?> FindStageActionTrackedAsync(int stageActionId) =>

            await _context.WorkflowStageActions.FirstOrDefaultAsync(a => a.StageActionId == stageActionId && !a.IsDeleted);



        private async Task<WorkflowStageActionViewer?> FindViewerAsync(int id) =>

            await _context.WorkflowStageActionViewers.AsNoTracking().FirstOrDefaultAsync(v => v.StageActionViewerId == id && !v.IsDeleted);



        private async Task<WorkflowStageActionViewer?> FindViewerTrackedAsync(int id) =>

            await _context.WorkflowStageActionViewers.FirstOrDefaultAsync(v => v.StageActionViewerId == id && !v.IsDeleted);



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode, int ModuleId, int? TenantId)> GetStageContextAsync(int stageId)

        {

            var stage = await _context.WorkflowModuleStages.AsNoTracking()

                .Include(s => s.Module)

                .FirstOrDefaultAsync(s => s.StageId == stageId && !s.IsDeleted);

            if (stage == null) return (false, "Not found.", HttpStatusCode.NotFound, 0, null);

            var tenantId = stage.Module?.TenantId;

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, tenantId);

            if (!access.Ok) return (false, access.Error, HttpStatusCode.Forbidden, stage.ModuleId, tenantId);

            return (true, null, HttpStatusCode.OK, stage.ModuleId, tenantId);

        }



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidateStageActionReferencesAsync(

            int actionId, int? nextStageId, int moduleId, int? tenantId, int stageId, int? assigneeTypeId, int? assignmentValue)

        {

            if (await FindActionAsync(actionId) == null)

                return (false, "Action not found.", HttpStatusCode.BadRequest);



            if (nextStageId.HasValue)

            {

                var next = await _context.WorkflowModuleStages.AsNoTracking()

                    .FirstOrDefaultAsync(s => s.StageId == nextStageId.Value && !s.IsDeleted);

                if (next == null) return (false, "Next stage not found.", HttpStatusCode.BadRequest);

                if (next.ModuleId != moduleId) return (false, "Next stage must belong to the same module.", HttpStatusCode.BadRequest);

            }

            return await ValidateAssigneeAsync(tenantId, assigneeTypeId, assignmentValue);

        }



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidateStageAssigneeAsync(int? tenantId, int? assigneeTypeId, int? assignmentValue)

        {

            if (!assigneeTypeId.HasValue && !assignmentValue.HasValue) return (true, null, HttpStatusCode.OK);

            if (!assigneeTypeId.HasValue)

                return (false, "Assignee type is required when assignment value is provided.", HttpStatusCode.BadRequest);

            if (assigneeTypeId == (int)Enums.WorkflowPartyType.User)

            {

                if (assignmentValue.HasValue)

                    return (false, "User-based stages do not use assignment value at design time; the user is assigned when the workflow runs.", HttpStatusCode.BadRequest);

                return (true, null, HttpStatusCode.OK);

            }

            if (!assignmentValue.HasValue)

                return (false, "Role is required for role-based stage assignment.", HttpStatusCode.BadRequest);

            return await ValidatePartyAsync(tenantId, assigneeTypeId.Value, assignmentValue.Value, "Stage assignee");

        }



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidateAssigneeAsync(int? tenantId, int? assigneeTypeId, int? assignmentValue)

        {

            if (!assigneeTypeId.HasValue && !assignmentValue.HasValue) return (true, null, HttpStatusCode.OK);

            if (!assigneeTypeId.HasValue || !assignmentValue.HasValue)

                return (false, "Assignee type and assignment value must both be set.", HttpStatusCode.BadRequest);

            return await ValidatePartyAsync(tenantId, assigneeTypeId.Value, assignmentValue.Value, "Assignee");

        }



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidateViewerAsync(int? tenantId, int viewerTypeId, int viewerValue) =>

            await ValidatePartyAsync(tenantId, viewerTypeId, viewerValue, "Viewer");



        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidatePartyAsync(int? tenantId, int partyTypeId, int partyValue, string label)

        {

            if (partyTypeId != (int)WorkflowPartyType.Role && partyTypeId != (int)WorkflowPartyType.User)

                return (false, $"{label} type is invalid.", HttpStatusCode.BadRequest);



            if (partyTypeId == (int)WorkflowPartyType.Role)

            {

                var exists = await _context.ApplicationRoles.AsNoTracking()

                    .AnyAsync(r => r.Id == partyValue && r.TenantId == tenantId);

                if (!exists) return (false, $"{label} role not found for tenant.", HttpStatusCode.BadRequest);

            }

            else

            {

                var exists = await _context.ApplicationUsers.AsNoTracking()

                    .AnyAsync(u => u.Id == partyValue && u.TenantId == tenantId);

                if (!exists) return (false, $"{label} user not found for tenant.", HttpStatusCode.BadRequest);

            }



            return (true, null, HttpStatusCode.OK);

        }



        private async Task<string?> ResolvePartyDisplayNameAsync(int? tenantId, int partyTypeId, int partyValue)

        {

            if (partyTypeId == (int)WorkflowPartyType.Role)

            {

                return await _context.ApplicationRoles.AsNoTracking()

                    .Where(r => r.Id == partyValue && r.TenantId == tenantId)

                    .Select(r => r.DisplayName)

                    .FirstOrDefaultAsync();

            }



            return await _context.ApplicationUsers.AsNoTracking()

                .Where(u => u.Id == partyValue && u.TenantId == tenantId)

                .Select(u => u.FullName)

                .FirstOrDefaultAsync();

        }



        private async Task<WorkflowModuleDto?> MapModuleAsync(int moduleId) =>

            await _context.WorkflowModules.AsNoTracking()

                .Where(m => m.ModuleId == moduleId && !m.IsDeleted)

                .Select(m => new WorkflowModuleDto

                {

                    ModuleId = m.ModuleId,

                    ModuleNameEn = m.ModuleNameEn,

                    ModuleNameAr = m.ModuleNameAr,

                    DisplayOrder = m.DisplayOrder,

                    IsReOpen = m.IsReOpen,

                    TenantId = m.TenantId,

                    TenantName = m.Tenant != null ? m.Tenant.CompanyName : null,

                    IsActive = m.IsActive

                }).FirstOrDefaultAsync();



        private async Task<WorkflowModuleStageDto?> MapStageAsync(int stageId)

        {

            var stage = await _context.WorkflowModuleStages.AsNoTracking()

                .Include(s => s.Module)

                .FirstOrDefaultAsync(s => s.StageId == stageId && !s.IsDeleted);

            return stage == null ? null : await MapStageDtoAsync(stage, stage.Module?.TenantId);

        }



        private async Task<WorkflowModuleStageDto> MapStageDtoAsync(WorkflowModuleStage s, int? tenantId)

        {

            var dto = MapStageEntity(s);

            if (s.AssigneeTypeId == (int)Enums.WorkflowPartyType.User)

                dto.AssigneeDisplayName = "User (assigned at runtime)";

            else if (s.AssigneeTypeId.HasValue && s.AssignmentValue.HasValue)

                dto.AssigneeDisplayName = await ResolvePartyDisplayNameAsync(tenantId, s.AssigneeTypeId.Value, s.AssignmentValue.Value);

            return dto;

        }



        private static WorkflowModuleStageDto MapStageEntity(WorkflowModuleStage s) => new()

        {

            StageId = s.StageId,

            ModuleId = s.ModuleId,

            StageNameEn = s.StageNameEn,

            StageNameAr = s.StageNameAr,

            SequenceNo = s.SequenceNo,

            DueInHours = s.DueInHours,

            VersionNo = s.VersionNo,

            EmailTemplateId = s.EmailTemplateId,

            IsFinalApprover = s.IsFinalApprover,

            AcknowledgementTemplateId = s.AcknowledgementTemplateId,

            IsClosingStage = s.IsClosingStage,

            IsInitialStage = s.IsInitialStage,

            IsLogOpen = s.IsLogOpen,

            IsTATExtension = s.IsTATExtension,

            AssigneeTypeId = s.AssigneeTypeId,

            AssignmentValue = s.AssignmentValue,

            IsActive = s.IsActive

        };



        private async Task ClearOtherInitialStagesAsync(int moduleId, int keepStageId)

        {

            var others = await _context.WorkflowModuleStages

                .Where(s => s.ModuleId == moduleId && s.StageId != keepStageId && !s.IsDeleted && s.IsInitialStage)

                .ToListAsync();

            foreach (var s in others)

                s.IsInitialStage = false;

        }



        private static WorkflowActionDto MapActionEntity(WorkflowActionDefinition x) => new()

        {

            ActionId = x.ActionId,

            BeforeActionEn = x.BeforeActionEn,

            BeforeActionAr = x.BeforeActionAr,

            AfterActionEn = x.AfterActionEn,

            AfterActionAr = x.AfterActionAr,

            BaseCssClass = x.BaseCssClass,

            IsNotesRequired = x.IsNotesRequired,

            IsActive = x.IsActive

        };



        private async Task<WorkflowStageActionDto?> MapStageActionAsync(int stageActionId)

        {

            var entity = await _context.WorkflowStageActions.AsNoTracking()

                .Include(a => a.Action)

                .Include(a => a.Stage).ThenInclude(s => s.Module)

                .FirstOrDefaultAsync(a => a.StageActionId == stageActionId && !a.IsDeleted);

            if (entity == null) return null;



            var nextStageName = entity.NextSageId.HasValue

                ? await _context.WorkflowModuleStages.AsNoTracking()

                    .Where(s => s.StageId == entity.NextSageId.Value && !s.IsDeleted)

                    .Select(s => s.StageNameEn)

                    .FirstOrDefaultAsync()

                : null;



            return await MapStageActionFromEntityAsync(entity,

                entity.NextSageId.HasValue && nextStageName != null

                    ? new Dictionary<int, string> { { entity.NextSageId.Value, nextStageName } }

                    : new Dictionary<int, string>(),

                entity.Stage?.Module?.TenantId);

        }



        private async Task<WorkflowStageActionDto> MapStageActionFromEntityAsync(

            WorkflowStageAction entity, IReadOnlyDictionary<int, string> nextStageNames, int? tenantId)

        {

            string? assigneeDisplay = null;

            if (entity.AssigneeTypeId.HasValue && entity.AssignmentValue.HasValue)

            {

                assigneeDisplay = await ResolvePartyDisplayNameAsync(tenantId, entity.AssigneeTypeId.Value, entity.AssignmentValue.Value);

            }



            string? nextStageName = null;

            if (entity.NextSageId.HasValue)

                nextStageNames.TryGetValue(entity.NextSageId.Value, out nextStageName);



            return new WorkflowStageActionDto

            {

                StageActionId = entity.StageActionId,

                StageId = entity.StageId,

                ActionId = entity.ActionId,

                ActionLabel = entity.Action != null ? entity.Action.AfterActionEn : null,

                StageActionDisplayName = entity.StageActionDisplayName,

                StageActionDisplayNameAr = entity.StageActionDisplayNameAr,

                EmailTemplateId = entity.EmailTemplateId,

                DisplayOrderNo = entity.DisplayOrderNo,

                CssClass = entity.CssClass,

                NextSageId = entity.NextSageId,

                NextStageName = nextStageName,

                AssigneeTypeId = entity.AssigneeTypeId,

                AssignmentValue = entity.AssignmentValue,

                AssigneeDisplayName = assigneeDisplay,

                AcknowledgementTemplateId = entity.AcknowledgementTemplateId,

                ActionPopUpRenderingComponents = entity.ActionPopUpRenderingComponents,

                TemplateId = entity.TemplateId,

                SMSTemplateId = entity.SMSTemplateId,

                IsActive = entity.IsActive

            };

        }



        private async Task<WorkflowStageActionViewerDto?> MapViewerAsync(int stageActionViewerId, int? tenantId)

        {

            var entity = await FindViewerAsync(stageActionViewerId);

            return entity == null ? null : await MapViewerFromEntityAsync(entity, tenantId);

        }



        private async Task<WorkflowStageActionViewerDto> MapViewerFromEntityAsync(WorkflowStageActionViewer entity, int? tenantId)
        {
            return new WorkflowStageActionViewerDto
            {
                StageActionViewerId = entity.StageActionViewerId,
                StageId = entity.StageId,
                ViewerTypeId = entity.ViewerTypeId,
                ViewerValue = entity.ViewerValue,
                ViewerDisplayName = await ResolvePartyDisplayNameAsync(tenantId, entity.ViewerTypeId, entity.ViewerValue),
                IsActive = entity.IsActive
            };
        }



        #endregion

    }

}

