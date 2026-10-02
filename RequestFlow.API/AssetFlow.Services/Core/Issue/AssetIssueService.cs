using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Issue;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Issue;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core.Issue
{
    public class AssetIssueService : IAssetIssueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;

        public AssetIssueService(
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

        public async Task<ResponseDto<AssetIssueDto>> CreateAsync(CreateAssetIssueDto dto)
        {
            var response = new ResponseDto<AssetIssueDto>();
            if (dto.AssetId <= 0 || dto.IssueCategoryId <= 0 || string.IsNullOrWhiteSpace(dto.Description))
            { response.AddError("Asset, category, and description are required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var relatedError = await ValidateRelatedTenantsAsync(tenantResult.TenantId, dto.AssetId, dto.IssueCategoryId);
            if (relatedError != null) { response.AddError(relatedError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var asset = await _context.Assets.FindAsync(dto.AssetId);
            if (asset == null || !asset.IsActive) { response.AddError("Asset not found or inactive."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) { response.AddError("User context is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var locationPath = await IssueLocationPathHelper.BuildPathAsync(_context, asset.LocationId);
            var entity = new AssetIssue
            {
                TenantId = tenantResult.TenantId,
                IssueNumber = await GenerateIssueNumberAsync(tenantResult.TenantId),
                AssetId = dto.AssetId,
                LocationId = asset.LocationId,
                LocationDisplayPath = locationPath,
                ReportedByUserId = userId.Value,
                ReportedAt = dto.ReportedAt ?? DateTime.UtcNow,
                IssueCategoryId = dto.IssueCategoryId,
                Priority = dto.Priority,
                Description = dto.Description.Trim(),
                AssetStatusAtReport = dto.AssetStatusAtReport,
                ImmediateAction = dto.ImmediateAction?.Trim() ?? string.Empty,
                Status = (int)Enums.IssueStatus.New,
                ResolutionRemarks = string.Empty
            };
            _context.AssetIssues.Add(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetIssueDto>> UpdateAsync(UpdateAssetIssueDto dto)
        {
            var response = new ResponseDto<AssetIssueDto>();
            var entity = await _context.AssetIssues.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (entity.Status == (int)Enums.IssueStatus.Resolved || entity.Status == (int)Enums.IssueStatus.Cancelled)
            { response.AddError("Resolved or cancelled issues cannot be edited."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var catError = await ValidateCategoryTenantAsync(entity.TenantId, dto.IssueCategoryId);
            if (catError != null) { response.AddError(catError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.IssueCategoryId = dto.IssueCategoryId;
            entity.Priority = dto.Priority;
            entity.Description = dto.Description?.Trim() ?? string.Empty;
            entity.AssetStatusAtReport = dto.AssetStatusAtReport;
            entity.ImmediateAction = dto.ImmediateAction?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetIssueDto>> ChangeStatusAsync(ChangeAssetIssueStatusDto dto)
        {
            var response = new ResponseDto<AssetIssueDto>();
            var entity = await _context.AssetIssues.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            entity.Status = dto.Status;
            if (dto.Status == (int)Enums.IssueStatus.Resolved)
            {
                entity.ResolvedAt = DateTime.UtcNow;
                entity.ResolvedByUserId = userId;
                entity.ResolutionRemarks = dto.ResolutionRemarks?.Trim() ?? string.Empty;
            }
            else if (dto.Status == (int)Enums.IssueStatus.Cancelled)
            {
                entity.ResolutionRemarks = dto.ResolutionRemarks?.Trim() ?? string.Empty;
            }
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetIssueDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<AssetIssueDto>();
            var dto = await MapDetailAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(AssetIssueFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.AssetIssues.AsQueryable(), _tenantProvider, model.TenantId);
            query = query.Include(x => x.Asset).Include(x => x.Location).Include(x => x.IssueCategory).Include(x => x.ReportedByUser).Include(x => x.Tenant);
            if (model.AssetId.HasValue) query = query.Where(x => x.AssetId == model.AssetId);
            if (model.IssueCategoryId.HasValue) query = query.Where(x => x.IssueCategoryId == model.IssueCategoryId);
            if (model.Priority.HasValue) query = query.Where(x => x.Priority == model.Priority);
            if (model.Status.HasValue) query = query.Where(x => x.Status == model.Status);
            if (model.LocationId.HasValue) query = query.Where(x => x.LocationId == model.LocationId);
            if (model.ReportedByUserId.HasValue) query = query.Where(x => x.ReportedByUserId == model.ReportedByUserId);
            if (model.ReportedFrom.HasValue) query = query.Where(x => x.ReportedAt >= model.ReportedFrom);
            if (model.ReportedTo.HasValue) query = query.Where(x => x.ReportedAt <= model.ReportedTo);
            if (model.StartDate.HasValue) query = query.Where(x => x.ReportedAt >= model.StartDate);
            if (model.EndDate.HasValue) query = query.Where(x => x.ReportedAt <= model.EndDate);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x =>
                    x.IssueNumber.Contains(s) ||
                    x.Description.Contains(s) ||
                    x.Asset.AssetCode.Contains(s) ||
                    x.Asset.Name.Contains(s) ||
                    (x.Asset.SerialNumber != null && x.Asset.SerialNumber.Contains(s)));
            }
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "ReportedAt" : model.OrderByProp;
            if (model.SortDirection == (int)Enums.OrderBy.Descending) order += " descending";
            var page = await query.OrderBy(order).Select(ProjectList()).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.AssetIssues.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<ResponseDto<IssueAttachmentDto>> AddAttachmentAsync(int issueId, IFormFile file)
        {
            var response = new ResponseDto<IssueAttachmentDto>();
            var issue = await _context.AssetIssues.FindAsync(issueId);
            if (issue == null) { response.AddError("Issue not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, issue.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (file == null || file.Length == 0) { response.AddError("File is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantSegment = issue.TenantId?.ToString() ?? "system";
            var folder = Path.Combine(_environment.ContentRootPath, "uploads", "issues", tenantSegment, issueId.ToString());
            Directory.CreateDirectory(folder);
            var safeName = Path.GetFileName(file.FileName);
            var storedName = $"{Guid.NewGuid():N}_{safeName}";
            var fullPath = Path.Combine(folder, storedName);
            await using (var stream = File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }
            var relative = Path.Combine("uploads", "issues", tenantSegment, issueId.ToString(), storedName).Replace('\\', '/');
            var attachment = new IssueAttachment
            {
                AssetIssueId = issueId,
                FileName = safeName,
                ContentType = file.ContentType ?? "application/octet-stream",
                StoragePath = relative,
                FileSizeBytes = file.Length
            };
            _context.IssueAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            response.Result = new IssueAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                DownloadUrl = $"/api/assetissue/attachments/{attachment.Id}/download"
            };
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAttachmentAsync(int attachmentId)
        {
            var response = new ResponseDto<bool>();
            var attachment = await _context.IssueAttachments.Include(x => x.AssetIssue).FirstOrDefaultAsync(x => x.Id == attachmentId);
            if (attachment == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, attachment.AssetIssue.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            attachment.IsDeleted = true;
            attachment.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentStreamAsync(int attachmentId)
        {
            var attachment = await _context.IssueAttachments.Include(x => x.AssetIssue).FirstOrDefaultAsync(x => x.Id == attachmentId && !x.IsDeleted);
            if (attachment == null) return null;
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, attachment.AssetIssue.TenantId);
            if (!access.Ok) return null;
            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath)) return null;
            var stream = File.OpenRead(fullPath);
            return (stream, attachment.ContentType, attachment.FileName);
        }

        private async Task<string> GenerateIssueNumberAsync(int? tenantId)
        {
            const string prefix = "ISS-";
            var last = await _context.AssetIssues
                .Where(x => x.TenantId == tenantId && x.IssueNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.IssueNumber)
                .FirstOrDefaultAsync();
            var next = 1;
            if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length && int.TryParse(last[prefix.Length..], out var n))
                next = n + 1;
            return $"{prefix}{next:D5}";
        }

        private async Task<string?> ValidateRelatedTenantsAsync(int? tenantId, int assetId, int issueCategoryId)
        {
            if (!tenantId.HasValue) return "Tenant is required.";
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || !asset.IsActive) return "Asset not found or inactive.";
            if (asset.TenantId != tenantId) return "Asset does not belong to the selected tenant.";
            return await ValidateCategoryTenantAsync(tenantId, issueCategoryId);
        }

        private async Task<string?> ValidateCategoryTenantAsync(int? tenantId, int issueCategoryId)
        {
            var cat = await _context.IssueCategories.FindAsync(issueCategoryId);
            if (cat == null || !cat.IsActive) return "Issue category not found or inactive.";
            if (cat.TenantId != tenantId) return "Issue category does not belong to the selected tenant.";
            return null;
        }

        private static System.Linq.Expressions.Expression<Func<AssetIssue, AssetIssueDto>> ProjectList() =>
            x => new AssetIssueDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                IssueNumber = x.IssueNumber,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                AssetSerialNumber = x.Asset.SerialNumber,
                AssetCriticality = x.Asset.Criticality,
                LocationId = x.LocationId,
                LocationName = x.Location.Name,
                LocationDisplayPath = x.LocationDisplayPath,
                ReportedByUserId = x.ReportedByUserId,
                ReportedByUserName = x.ReportedByUser.FullName ?? x.ReportedByUser.UserName,
                ReportedAt = x.ReportedAt,
                IssueCategoryId = x.IssueCategoryId,
                IssueCategoryName = x.IssueCategory.Name,
                Priority = x.Priority,
                Description = x.Description,
                AssetStatusAtReport = x.AssetStatusAtReport,
                ImmediateAction = x.ImmediateAction,
                Status = x.Status,
                ResolvedAt = x.ResolvedAt,
                ResolvedByUserId = x.ResolvedByUserId,
                WorkOrderId = x.WorkOrderId
            };

        private async Task<AssetIssueDto?> MapDetailAsync(int id)
        {
            var dto = await _context.AssetIssues
                .Include(x => x.Asset)
                .Include(x => x.Location)
                .Include(x => x.IssueCategory)
                .Include(x => x.ReportedByUser)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.Tenant)
                .Include(x => x.Attachments)
                .Where(x => x.Id == id)
                .Select(x => new AssetIssueDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                    IssueNumber = x.IssueNumber,
                    AssetId = x.AssetId,
                    AssetCode = x.Asset.AssetCode,
                    AssetName = x.Asset.Name,
                    AssetSerialNumber = x.Asset.SerialNumber,
                    AssetCriticality = x.Asset.Criticality,
                    LocationId = x.LocationId,
                    LocationName = x.Location.Name,
                    LocationDisplayPath = x.LocationDisplayPath,
                    ReportedByUserId = x.ReportedByUserId,
                    ReportedByUserName = x.ReportedByUser.FullName ?? x.ReportedByUser.UserName,
                    ReportedAt = x.ReportedAt,
                    IssueCategoryId = x.IssueCategoryId,
                    IssueCategoryName = x.IssueCategory.Name,
                    Priority = x.Priority,
                    Description = x.Description,
                    AssetStatusAtReport = x.AssetStatusAtReport,
                    ImmediateAction = x.ImmediateAction,
                    Status = x.Status,
                    ResolvedAt = x.ResolvedAt,
                    ResolvedByUserId = x.ResolvedByUserId,
                    ResolvedByUserName = x.ResolvedByUser != null ? (x.ResolvedByUser.FullName ?? x.ResolvedByUser.UserName) : null,
                    ResolutionRemarks = x.ResolutionRemarks,
                    WorkOrderId = x.WorkOrderId,
                    Attachments = x.Attachments.Where(a => !a.IsDeleted && a.IsActive).Select(a => new IssueAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSizeBytes = a.FileSizeBytes,
                        DownloadUrl = $"/api/assetissue/attachments/{a.Id}/download"
                    }).ToList()
                }).FirstOrDefaultAsync();
            return dto;
        }
    }
}
