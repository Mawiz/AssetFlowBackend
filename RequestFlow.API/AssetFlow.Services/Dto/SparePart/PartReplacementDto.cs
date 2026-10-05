using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartReplacementDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int? OldAssetComponentId { get; set; }
        public string OldComponentName { get; set; }
        public string OldPartNumber { get; set; }
        public string OldSerialNumber { get; set; }
        public int NewPartId { get; set; }
        public string NewPartNumber { get; set; }
        public string NewPartName { get; set; }
        public string NewSerialNumber { get; set; }
        public int? NewAssetComponentId { get; set; }
        public decimal Quantity { get; set; }
        public string InstalledByUserName { get; set; }
        public DateTime InstalledAt { get; set; }
        public string RemovedByUserName { get; set; }
        public DateTime? RemovedAt { get; set; }
        public string RemovalReason { get; set; }
        public string FailureReason { get; set; }
        public string FromLocationName { get; set; }
        public string InstallationLocation { get; set; }
        public string Remarks { get; set; }
    }

    public class ValidatePartReplacementDto
    {
        public int WorkOrderId { get; set; }
        public int OldAssetComponentId { get; set; }
        public int? NewPartId { get; set; }
        public int? NewPartSerialNumberId { get; set; }
        public string NewSerialNumber { get; set; }
        public int? NewPartInventoryBatchId { get; set; }
        public decimal Quantity { get; set; } = 1;
    }

    public class PartReplacementValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public PartLookupSummaryDto NewPart { get; set; }
        public PartSerialSummaryDto Serial { get; set; }
        public decimal? AvailableQuantity { get; set; }
        public bool IsCompatible { get; set; }
        public string OldComponentName { get; set; }
        public string OldPartNumber { get; set; }
        public string OldSerialNumber { get; set; }
        public List<PartInstallationHistoryDto> PreviousInstallations { get; set; } = new();
    }

    public class PartLookupSummaryDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public bool IsSerialized { get; set; }
        public string Manufacturer { get; set; }
    }

    public class PartSerialSummaryDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string LocationName { get; set; }
        public int? LocationId { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string SupplierName { get; set; }
    }

    public class PartInstallationHistoryDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public DateTime InstalledAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public string WorkOrderNumber { get; set; }
    }

    public class ConfirmPartReplacementDto : ValidatePartReplacementDto
    {
        public string RemovalReason { get; set; }
        public string FailureReason { get; set; }
        public string InstallationLocation { get; set; }
        public string Remarks { get; set; }
        public bool MarkOldSerialFaulty { get; set; }
    }

    public class PartReplacementFilterDto : SearchViewDto
    {
        public int? WorkOrderId { get; set; }
        public int? AssetId { get; set; }
    }
}
