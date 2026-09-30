using AssetFlow.Services.Dto;



namespace AssetFlow.Services.Dto.SparePart

{

    public class PartInventoryDto

    {

        public int Id { get; set; }

        public int? TenantId { get; set; }

        public string TenantName { get; set; }

        public int PartId { get; set; }

        public string PartNumber { get; set; }

        public string PartName { get; set; }

        public bool PartIsSerialized { get; set; }

        public int LocationId { get; set; }

        public string LocationName { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal FaultyQuantity { get; set; }

        public decimal QuarantineQuantity { get; set; }

        public decimal IssuedQuantity { get; set; }

        public DateTime? EarliestExpiryDate { get; set; }

        public bool IsLowStock { get; set; }

        public bool IsActive { get; set; }

    }



    public class PartInventoryDetailDto : PartInventoryDto

    {

        public List<PartInventoryBatchDto> Batches { get; set; } = new();

    }



    public class PartInventoryBatchDto

    {

        public int Id { get; set; }

        public int PartInventoryId { get; set; }

        public int PartId { get; set; }

        public string BatchReference { get; set; }

        public int? SupplierId { get; set; }

        public string SupplierName { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }

        public int? ExpectedLifeUnit { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal FaultyQuantity { get; set; }

        public decimal QuarantineQuantity { get; set; }

        public decimal IssuedQuantity { get; set; }

        public string Notes { get; set; }

        public bool IsActive { get; set; }

    }



    public class PartInventoryBatchDetailDto : PartInventoryBatchDto

    {

        public string PartNumber { get; set; }

        public string PartName { get; set; }

        public string LocationName { get; set; }

        public List<PartSerialNumberDto> SerialNumbers { get; set; } = new();

    }



    public class PartInventoryFilterDto : SearchViewDto

    {

        public int? PartId { get; set; }

        public int? LocationId { get; set; }

        public bool? LowStockOnly { get; set; }

    }



    public class PartReceiveLineDto

    {

        public int SupplierId { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }

        public int? ExpectedLifeUnit { get; set; }

        public DateTime? WarrantyStartDate { get; set; }

        public DateTime? WarrantyEndDate { get; set; }

        public string? Notes { get; set; }

        /// <summary>0 = auto internal serials, 1 = supplier serial scan mode (serialized only).</summary>

        public int ReceiptMode { get; set; }

        public List<string> SupplierSerialReferences { get; set; } = new();

    }



    public class PartReceiveStockDto

    {

        public int? TenantId { get; set; }

        public int PartId { get; set; }

        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        public List<PartReceiveLineDto> Lines { get; set; } = new();

    }



    public class PartReceiptDto

    {

        public int? TenantId { get; set; }

        public int PartId { get; set; }

        public int LocationId { get; set; }

        public int SupplierId { get; set; }

        public decimal Quantity { get; set; } = 1;

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }

        public int? ExpectedLifeUnit { get; set; }

        public DateTime? WarrantyStartDate { get; set; }

        public DateTime? WarrantyEndDate { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartBatchReceiptDto

    {

        public int? TenantId { get; set; }

        public int PartId { get; set; }

        public int LocationId { get; set; }

        public int SupplierId { get; set; }

        public int Quantity { get; set; } = 1;

        public int ReceiptMode { get; set; }

        public List<string> SupplierSerialReferences { get; set; } = new();

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }

        public int? ExpectedLifeUnit { get; set; }

        public DateTime? WarrantyStartDate { get; set; }

        public DateTime? WarrantyEndDate { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartBatchReceiptResultDto

    {

        public int ReceiptTransactionId { get; set; }

        public List<int> CreatedBatchIds { get; set; } = new();

        public List<string> GeneratedSerialNumbers { get; set; } = new();

    }



    public class PartTransferDto

    {

        public int? TenantId { get; set; }

        public int PartInventoryBatchId { get; set; }

        public int ToLocationId { get; set; }

        public decimal Quantity { get; set; }

        public List<int> PartSerialNumberIds { get; set; } = new();

        public string? Remarks { get; set; }

    }



    public class PartAdjustmentDto

    {

        public int? TenantId { get; set; }

        public int PartInventoryBatchId { get; set; }

        public decimal QuantityChange { get; set; }

        public string? Reason { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartIssueDto

    {

        public int? TenantId { get; set; }

        public int PartInventoryBatchId { get; set; }

        public decimal Quantity { get; set; }

        public List<int> PartSerialNumberIds { get; set; } = new();

        public int? IssuedToUserId { get; set; }

        public string? Reason { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartReturnDto

    {

        public int? TenantId { get; set; }

        public int PartInventoryBatchId { get; set; }

        public decimal Quantity { get; set; }

        public List<int> PartSerialNumberIds { get; set; } = new();

        public int? ReturnedFromUserId { get; set; }

        public int ToLocationId { get; set; }

        public string? Reason { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartInventoryStateChangeDto

    {

        public int? TenantId { get; set; }

        public int PartInventoryBatchId { get; set; }

        public decimal Quantity { get; set; }

        public List<int> PartSerialNumberIds { get; set; } = new();

        public string? Reason { get; set; }

        public string? Remarks { get; set; }

    }



    public class PartReturnToSupplierDto : PartInventoryStateChangeDto

    {

        public int SupplierId { get; set; }

    }

}


