using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartSerialNumberDto : CreatePartSerialNumberDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string LocationName { get; set; }
        public string TenantName { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierSerialReference { get; set; }
        public int? PartInventoryBatchId { get; set; }
        public int? OriginPartInventoryBatchId { get; set; }
        public string BatchReference { get; set; }
        /// <summary>True when the serial is still on this batch row (can be operated from the open batch view).</summary>
        public bool IsAtOpenBatch { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }
    }

    public class CreatePartSerialNumberDto
    {
        public int? TenantId { get; set; }
        public int PartId { get; set; }
        public string SerialNumber { get; set; }
        public int Status { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int? LocationId { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePartSerialNumberDto : CreatePartSerialNumberDto
    {
        public int Id { get; set; }
    }

    public class PartSerialNumberFilterDto : SearchViewDto
    {
        public int? PartId { get; set; }
    }

    public class NextPartSerialDto
    {
        public string SerialNumber { get; set; }
        public List<string> SerialNumbers { get; set; } = new();
    }
}
