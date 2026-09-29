using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartInventoryDto : CreatePartInventoryDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string LocationName { get; set; }
        public string SerialNumber { get; set; }
        public string TenantName { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class CreatePartInventoryDto
    {
        public int? TenantId { get; set; }
        public int PartId { get; set; }
        public int LocationId { get; set; }
        public int? PartSerialNumberId { get; set; }
        public decimal QuantityAvailable { get; set; }
        public decimal QuantityReserved { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePartInventoryDto : CreatePartInventoryDto
    {
        public int Id { get; set; }
    }

    public class PartInventoryFilterDto : SearchViewDto
    {
        public int? PartId { get; set; }
        public int? LocationId { get; set; }
        public bool? LowStockOnly { get; set; }
    }

    public class PartReceiptDto
    {
        public int? TenantId { get; set; }
        public int PartId { get; set; }
        public int LocationId { get; set; }
        public decimal Quantity { get; set; } = 1;
        public string SerialNumber { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string Remarks { get; set; }
    }

    public class PartTransferDto
    {
        public int? TenantId { get; set; }
        public int PartInventoryId { get; set; }
        public int ToLocationId { get; set; }
        public decimal Quantity { get; set; }
        public string Remarks { get; set; }
    }

    public class PartAdjustmentDto
    {
        public int? TenantId { get; set; }
        public int PartInventoryId { get; set; }
        public decimal QuantityChange { get; set; }
        public string Remarks { get; set; }
    }
}
