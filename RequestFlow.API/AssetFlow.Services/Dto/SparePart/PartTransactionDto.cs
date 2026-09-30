using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartTransactionDto : CreatePartTransactionDto
    {
        public int Id { get; set; }
        public int? PartInventoryId { get; set; }
        public int? PartInventoryBatchId { get; set; }
        public int? IssuedToUserId { get; set; }
        public int? ReturnedFromUserId { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string BatchReference { get; set; }
        public List<string> SerialNumbers { get; set; } = new();
        public string FromLocationName { get; set; }
        public string ToLocationName { get; set; }
        public int? PerformedByUserId { get; set; }
        public string PerformedByUserName { get; set; }
        public string IssuedToUserName { get; set; }
        public string ReturnedFromUserName { get; set; }
        public string TenantName { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePartTransactionDto
    {
        public int? TenantId { get; set; }
        public int PartId { get; set; }
        public int TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; }
        public string Remarks { get; set; }
    }

    public class PartTransactionFilterDto : SearchViewDto
    {
        public int? PartId { get; set; }
        public int? PartInventoryId { get; set; }
        public int? TransactionType { get; set; }
    }
}
