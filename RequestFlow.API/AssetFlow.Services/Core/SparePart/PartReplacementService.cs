using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Asset;
using AssetFlow.Data.Entities.SparePart;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;

namespace AssetFlow.Services.Core.SparePart
{
    public class PartReplacementService : IPartReplacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly int[] AllowedWoStatuses =
        {
            (int)Enums.WorkOrderStatus.Accepted,
            (int)Enums.WorkOrderStatus.InProgress,
            (int)Enums.WorkOrderStatus.WaitingForParts,
            (int)Enums.WorkOrderStatus.Reopened
        };

        public PartReplacementService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ResponseDto<PartReplacementValidationResultDto>> ValidateAsync(ValidatePartReplacementDto dto) =>
            ValidateInternalAsync(dto, false);

        public Task<ResponseDto<PartReplacementValidationResultDto>> LookupSerialAsync(int workOrderId, string serialNumber) =>
            ValidateInternalAsync(new ValidatePartReplacementDto
            {
                WorkOrderId = workOrderId,
                OldAssetComponentId = 0,
                NewSerialNumber = serialNumber,
                Quantity = 1
            }, true);

        public async Task<ResponseDto<PartReplacementDto>> ReplaceAsync(ConfirmPartReplacementDto dto)
        {
            var response = new ResponseDto<PartReplacementDto>();
            var validation = await ValidateInternalAsync(dto, false);
            if (!validation.Result.IsValid)
            {
                foreach (var e in validation.Result.Errors) response.AddError(e);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId);
            var oldComp = await _context.AssetComponents.FindAsync(dto.OldAssetComponentId);
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor) ?? 0;
            var now = DateTime.UtcNow;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var part = await _context.Parts.FindAsync(dto.NewPartId!.Value);
                PartSerialNumber? newSerial = null;
                PartInventoryBatch? batch = null;
                PartInventory? stock = null;
                decimal qty = dto.Quantity <= 0 ? 1 : dto.Quantity;

                if (part!.IsSerialized)
                {
                    newSerial = await ResolveNewSerialAsync(dto);
                    if (newSerial == null)
                    {
                        response.AddError("Replacement serial not found.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    batch = await _context.PartInventoryBatches
                        .Include(b => b.PartInventory)
                        .FirstOrDefaultAsync(b => b.Id == newSerial.PartInventoryBatchId);
                    if (batch == null)
                    {
                        response.AddError("Serial batch not found.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    stock = batch.PartInventory;
                    if (newSerial.Status != (int)Enums.PartSerialStatus.InStock)
                    {
                        response.AddError("Serial is not available for installation.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    batch.AvailableQuantity -= 1;
                    batch.TotalQuantity -= 1;
                    stock.AvailableQuantity -= 1;
                    stock.TotalQuantity -= 1;
                    newSerial.Status = (int)Enums.PartSerialStatus.Issued;
                    qty = 1;
                }
                else
                {
                    if (!dto.NewPartInventoryBatchId.HasValue)
                    {
                        response.AddError("Batch is required for non-serialized parts.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    batch = await _context.PartInventoryBatches.Include(b => b.PartInventory)
                        .FirstOrDefaultAsync(b => b.Id == dto.NewPartInventoryBatchId);
                    if (batch == null || batch.PartId != part.Id)
                    {
                        response.AddError("Invalid inventory batch.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    stock = batch.PartInventory;
                    if (batch.AvailableQuantity < qty)
                    {
                        response.AddError("Insufficient available quantity.");
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    batch.AvailableQuantity -= qty;
                    batch.TotalQuantity -= qty;
                    stock.AvailableQuantity -= qty;
                    stock.TotalQuantity -= qty;
                }

                oldComp!.CurrentStatus = (int)Enums.ComponentCurrentStatus.Removed;
                oldComp.IsActive = false;
                oldComp.Notes = AppendNote(oldComp.Notes, $"Removed via WO {wo!.WorkOrderNumber}: {dto.FailureReason}");

                var oldSerial = await FindSerialByNumberAsync(oldComp.TenantId, oldComp.SerialNumber);
                if (oldSerial != null)
                {
                    oldSerial.Status = dto.MarkOldSerialFaulty
                        ? (int)Enums.PartSerialStatus.Faulty
                        : (int)Enums.PartSerialStatus.Removed;
                }

                var newComponent = new AssetComponent
                {
                    TenantId = wo.TenantId,
                    AssetId = wo.AssetId,
                    ComponentCode = oldComp.ComponentCode,
                    ComponentName = part.PartName,
                    PartNumber = part.PartNumber,
                    SerialNumber = newSerial?.SerialNumber ?? string.Empty,
                    Manufacturer = part.Manufacturer ?? oldComp.Manufacturer,
                    InstallationDate = now,
                    ExpectedLifeValue = part.ExpectedLifeValue ?? oldComp.ExpectedLifeValue,
                    ExpectedLifeUnit = part.ExpectedLifeUnit ?? oldComp.ExpectedLifeUnit,
                    CurrentStatus = (int)Enums.ComponentCurrentStatus.Active,
                    SupplierName = part.SupplierName ?? oldComp.SupplierName,
                    WarrantyStartDate = newSerial?.WarrantyStartDate,
                    WarrantyEndDate = newSerial?.WarrantyEndDate,
                    InstallationLocation = dto.InstallationLocation ?? oldComp.InstallationLocation,
                    CurrentRunningHours = oldComp.CurrentRunningHours,
                    Notes = dto.Remarks ?? string.Empty,
                    IsActive = true
                };
                _context.AssetComponents.Add(newComponent);

                var replacement = new PartReplacement
                {
                    TenantId = wo.TenantId,
                    WorkOrderId = wo.Id,
                    AssetId = wo.AssetId,
                    OldAssetComponentId = oldComp.Id,
                    OldPartNumber = oldComp.PartNumber ?? string.Empty,
                    OldSerialNumber = oldComp.SerialNumber ?? string.Empty,
                    OldPartSerialNumberId = oldSerial?.Id,
                    NewPartId = part.Id,
                    NewPartSerialNumberId = newSerial?.Id,
                    NewPartInventoryBatchId = batch.Id,
                    Quantity = qty,
                    InstalledByUserId = userId,
                    InstalledAt = now,
                    RemovedByUserId = userId,
                    RemovedAt = now,
                    RemovalReason = dto.RemovalReason?.Trim() ?? string.Empty,
                    FailureReason = dto.FailureReason?.Trim() ?? string.Empty,
                    FromLocationId = stock?.LocationId,
                    InstallationLocation = dto.InstallationLocation ?? oldComp.InstallationLocation ?? string.Empty,
                    Remarks = dto.Remarks?.Trim() ?? string.Empty
                };
                _context.PartReplacements.Add(replacement);
                await _context.SaveChangesAsync();

                newComponent.PartNumber = part.PartNumber;
                replacement.NewAssetComponentId = newComponent.Id;

                var txn = new PartTransaction
                {
                    TenantId = wo.TenantId,
                    PartId = part.Id,
                    PartInventoryId = stock!.Id,
                    PartInventoryBatchId = batch.Id,
                    TransactionType = (int)Enums.PartTransactionType.Issue,
                    Quantity = qty,
                    FromLocationId = stock.LocationId,
                    ToLocationId = null,
                    TransactionDate = now,
                    PerformedByUserId = userId,
                    IssuedToUserId = userId,
                    WorkOrderId = wo.Id,
                    AssetId = wo.AssetId,
                    PartReplacementId = replacement.Id,
                    Reason = "Part replacement / installation",
                    Remarks = dto.Remarks ?? string.Empty,
                    IsActive = true
                };
                _context.PartTransactions.Add(txn);
                await _context.SaveChangesAsync();

                if (newSerial != null)
                {
                    _context.PartTransactionSerials.Add(new PartTransactionSerial
                    {
                        PartTransactionId = txn.Id,
                        PartSerialNumberId = newSerial.Id,
                        IsActive = true
                    });
                }

                replacement.PartTransactionId = txn.Id;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.Result = await MapAsync(replacement.Id);
                return response;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ResponseDto<PartReplacementDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<PartReplacementDto>();
            var dto = await MapAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<PartReplacementDto>>> GetByWorkOrderAsync(int workOrderId)
        {
            var response = new ResponseDto<List<PartReplacementDto>>();
            var wo = await _context.WorkOrders.FindAsync(workOrderId);
            if (wo == null) { response.AddError("Work order not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, wo.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var list = await _context.PartReplacements
                .Where(x => x.WorkOrderId == workOrderId && !x.IsDeleted)
                .OrderByDescending(x => x.InstalledAt)
                .Select(x => x.Id)
                .ToListAsync();
            var dtos = new List<PartReplacementDto>();
            foreach (var id in list) dtos.Add(await MapAsync(id));
            response.Result = dtos;
            return response;
        }

        private async Task<ResponseDto<PartReplacementValidationResultDto>> ValidateInternalAsync(
            ValidatePartReplacementDto dto, bool serialLookupOnly)
        {
            var response = new ResponseDto<PartReplacementValidationResultDto>();
            var result = new PartReplacementValidationResultDto();

            var woError = await ValidateWorkOrderAsync(dto.WorkOrderId);
            if (woError != null) { result.Errors.Add(woError); result.IsValid = false; response.Result = result; return response; }

            var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId);
            var engineerError = ValidateEngineer(wo!);
            if (engineerError != null) { result.Errors.Add(engineerError); result.IsValid = false; response.Result = result; return response; }

            AssetComponent? oldComp = null;
            if (dto.OldAssetComponentId > 0)
            {
                oldComp = await _context.AssetComponents.FindAsync(dto.OldAssetComponentId);
                if (oldComp == null || !oldComp.IsActive)
                {
                    result.Errors.Add("Old component not found or not active.");
                }
                else if (oldComp.AssetId != wo!.AssetId)
                {
                    result.Errors.Add("Component does not belong to the work order asset.");
                }
                else if (oldComp.TenantId != wo.TenantId)
                {
                    result.Errors.Add("Component tenant mismatch.");
                }
                else
                {
                    result.OldComponentName = oldComp.ComponentName;
                    result.OldPartNumber = oldComp.PartNumber;
                    result.OldSerialNumber = oldComp.SerialNumber;
                }
            }
            else if (!serialLookupOnly)
            {
                result.Errors.Add("Old component is required.");
            }
            else if (serialLookupOnly && dto.OldAssetComponentId <= 0)
            {
                // serial-only lookup — old component not required
            }

            Part? part = null;
            PartSerialNumber? serial = null;
            if (!string.IsNullOrWhiteSpace(dto.NewSerialNumber))
            {
                serial = await FindSerialByNumberAsync(wo!.TenantId, dto.NewSerialNumber.Trim());
                if (serial == null) result.Errors.Add("Serial number not found.");
                else
                {
                    dto.NewPartSerialNumberId = serial.Id;
                    dto.NewPartId = serial.PartId;
                    part = await _context.Parts.FindAsync(serial.PartId);
                }
            }
            else if (dto.NewPartSerialNumberId.HasValue)
            {
                serial = await _context.PartSerialNumbers.Include(s => s.Location).FirstOrDefaultAsync(s => s.Id == dto.NewPartSerialNumberId);
                if (serial == null) result.Errors.Add("Serial not found.");
                else { dto.NewPartId = serial.PartId; part = await _context.Parts.FindAsync(serial.PartId); }
            }
            else if (dto.NewPartId.HasValue)
            {
                part = await _context.Parts.FindAsync(dto.NewPartId);
            }

            if (part == null && (dto.NewPartId.HasValue || !string.IsNullOrWhiteSpace(dto.NewSerialNumber)))
                result.Errors.Add("Replacement part not found.");
            else if (part != null)
            {
                if (part.TenantId != wo!.TenantId) result.Errors.Add("Part belongs to another tenant.");
                result.NewPart = new PartLookupSummaryDto
                {
                    Id = part.Id,
                    PartNumber = part.PartNumber,
                    PartName = part.PartName,
                    IsSerialized = part.IsSerialized,
                    Manufacturer = part.Manufacturer
                };

                if (oldComp != null)
                {
                    result.IsCompatible = PartsCompatible(oldComp.PartNumber, part.PartNumber);
                    if (!result.IsCompatible) result.Errors.Add("Replacement part is not compatible with the existing component (part number mismatch).");
                }

                if (part.IsSerialized && serial != null)
                {
                    if (serial.Status != (int)Enums.PartSerialStatus.InStock)
                        result.Errors.Add($"Part serial is not available (status: {serial.Status}).");
                    if (await IsSerialInstalledOnAssetAsync(serial.SerialNumber, wo.TenantId))
                        result.Errors.Add("Serial is already installed on an asset.");
                    if (batchExpired(serial)) result.Errors.Add("Batch/serial is expired.");

                    result.Serial = MapSerialSummary(serial);
                    result.PreviousInstallations = await GetInstallationHistoryAsync(serial.Id);
                }
                else if (!part.IsSerialized)
                {
                    if (!dto.NewPartInventoryBatchId.HasValue)
                        result.Errors.Add("Inventory batch is required for non-serialized parts.");
                    else
                    {
                        var batch = await _context.PartInventoryBatches.FindAsync(dto.NewPartInventoryBatchId);
                        if (batch == null || batch.PartId != part.Id)
                            result.Errors.Add("Invalid batch for selected part.");
                        else
                        {
                            result.AvailableQuantity = batch.AvailableQuantity;
                            if (batch.AvailableQuantity < (dto.Quantity <= 0 ? 1 : dto.Quantity))
                                result.Errors.Add("Insufficient quantity in batch.");
                            if (batch.ExpiryDate.HasValue && batch.ExpiryDate < DateTime.UtcNow.Date)
                                result.Errors.Add("Batch is expired.");
                        }
                    }
                }
            }

            result.IsValid = result.Errors.Count == 0;
            response.Result = result;
            return response;
        }

        private static bool batchExpired(PartSerialNumber serial) =>
            serial.ExpiryDate.HasValue && serial.ExpiryDate < DateTime.UtcNow.Date;

        private async Task<string?> ValidateWorkOrderAsync(int workOrderId)
        {
            var wo = await _context.WorkOrders.FindAsync(workOrderId);
            if (wo == null || !wo.IsActive) return "Work order not found.";
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, wo.TenantId);
            if (!access.Ok) return access.Error;
            if (!AllowedWoStatuses.Contains(wo.Status))
                return "Work order is not in a valid state for part replacement.";
            if (wo.Status == (int)Enums.WorkOrderStatus.Closed || wo.Status == (int)Enums.WorkOrderStatus.Cancelled)
                return "Work order is closed or cancelled.";
            return null;
        }

        private string? ValidateEngineer(WorkOrderEntity wo)
        {
            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            if (!userId.HasValue) return "User context required.";
            if (wo.AssignedToUserId.HasValue && wo.AssignedToUserId != userId)
                return "Only the assigned engineer can perform part replacement on this work order.";
            return null;
        }

        private static bool PartsCompatible(string? oldPartNumber, string? newPartNumber)
        {
            if (string.IsNullOrWhiteSpace(oldPartNumber) || string.IsNullOrWhiteSpace(newPartNumber)) return false;
            return string.Equals(oldPartNumber.Trim(), newPartNumber.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private async Task<PartSerialNumber?> FindSerialByNumberAsync(int? tenantId, string? serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return null;
            return await _context.PartSerialNumbers
                .Include(s => s.Location)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SerialNumber == serialNumber.Trim() && !s.IsDeleted);
        }

        private async Task<PartSerialNumber?> ResolveNewSerialAsync(ConfirmPartReplacementDto dto)
        {
            if (dto.NewPartSerialNumberId.HasValue)
                return await _context.PartSerialNumbers.FindAsync(dto.NewPartSerialNumberId.Value);
            if (!string.IsNullOrWhiteSpace(dto.NewSerialNumber))
            {
                var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId);
                return await FindSerialByNumberAsync(wo?.TenantId, dto.NewSerialNumber);
            }
            return null;
        }

        private async Task<bool> IsSerialInstalledOnAssetAsync(string serialNumber, int? tenantId)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return false;
            return await _context.AssetComponents.AnyAsync(c =>
                c.TenantId == tenantId &&
                c.SerialNumber == serialNumber &&
                c.IsActive &&
                c.CurrentStatus == (int)Enums.ComponentCurrentStatus.Active &&
                !c.IsDeleted);
        }

        private async Task<List<PartInstallationHistoryDto>> GetInstallationHistoryAsync(int serialId)
        {
            return await _context.PartReplacements
                .Include(r => r.Asset)
                .Include(r => r.WorkOrder)
                .Where(r => r.NewPartSerialNumberId == serialId && !r.IsDeleted)
                .OrderByDescending(r => r.InstalledAt)
                .Take(10)
                .Select(r => new PartInstallationHistoryDto
                {
                    AssetId = r.AssetId,
                    AssetCode = r.Asset.AssetCode,
                    InstalledAt = r.InstalledAt,
                    RemovedAt = r.RemovedAt,
                    WorkOrderNumber = r.WorkOrder.WorkOrderNumber
                }).ToListAsync();
        }

        private static PartSerialSummaryDto MapSerialSummary(PartSerialNumber serial) =>
            new()
            {
                Id = serial.Id,
                SerialNumber = serial.SerialNumber,
                Status = serial.Status,
                StatusName = Enum.GetName(typeof(Enums.PartSerialStatus), serial.Status) ?? serial.Status.ToString(),
                ReceivedDate = serial.ReceivedDate,
                LocationId = serial.LocationId,
                LocationName = serial.Location?.Name,
                WarrantyStartDate = serial.WarrantyStartDate,
                WarrantyEndDate = serial.WarrantyEndDate,
                SupplierName = serial.Supplier?.Name ?? serial.SupplierSerialReference
            };

        private async Task<PartReplacementDto> MapAsync(int id)
        {
            return await _context.PartReplacements
                .Include(r => r.WorkOrder)
                .Include(r => r.Asset)
                .Include(r => r.OldAssetComponent)
                .Include(r => r.NewPart)
                .Include(r => r.NewPartSerialNumber)
                .Include(r => r.FromLocation)
                .Include(r => r.InstalledByUser)
                .Include(r => r.RemovedByUser)
                .Where(r => r.Id == id)
                .Select(r => new PartReplacementDto
                {
                    Id = r.Id,
                    TenantId = r.TenantId,
                    WorkOrderId = r.WorkOrderId,
                    WorkOrderNumber = r.WorkOrder.WorkOrderNumber,
                    AssetId = r.AssetId,
                    AssetCode = r.Asset.AssetCode,
                    AssetName = r.Asset.Name,
                    OldAssetComponentId = r.OldAssetComponentId,
                    OldComponentName = r.OldAssetComponent != null ? r.OldAssetComponent.ComponentName : null,
                    OldPartNumber = r.OldPartNumber,
                    OldSerialNumber = r.OldSerialNumber,
                    NewPartId = r.NewPartId,
                    NewPartNumber = r.NewPart.PartNumber,
                    NewPartName = r.NewPart.PartName,
                    NewSerialNumber = r.NewPartSerialNumber != null ? r.NewPartSerialNumber.SerialNumber : null,
                    NewAssetComponentId = r.NewAssetComponentId,
                    Quantity = r.Quantity,
                    InstalledByUserName = r.InstalledByUser.FullName ?? r.InstalledByUser.UserName,
                    InstalledAt = r.InstalledAt,
                    RemovedByUserName = r.RemovedByUser != null ? (r.RemovedByUser.FullName ?? r.RemovedByUser.UserName) : null,
                    RemovedAt = r.RemovedAt,
                    RemovalReason = r.RemovalReason,
                    FailureReason = r.FailureReason,
                    FromLocationName = r.FromLocation != null ? r.FromLocation.Name : null,
                    InstallationLocation = r.InstallationLocation,
                    Remarks = r.Remarks
                }).FirstAsync();
        }

        private static string AppendNote(string existing, string line)
        {
            if (string.IsNullOrWhiteSpace(existing)) return line;
            return existing + Environment.NewLine + line;
        }
    }
}
