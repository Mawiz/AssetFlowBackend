using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartTransactionDto : CreatePartTransactionDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string SerialNumber { get; set; }
        public string FromLocationName { get; set; }
        public string ToLocationName { get; set; }
        public int? PerformedByUserId { get; set; }
        public string PerformedByUserName { get; set; }
        public string TenantName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePartTransactionDto
    {
        public int? TenantId { get; set; }
        public int PartId { get; set; }
        public int? PartSerialNumberId { get; set; }
        public int TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string Remarks { get; set; }
    }

    public class PartTransactionFilterDto : SearchViewDto
    {
        public int? PartId { get; set; }
        public int? TransactionType { get; set; }
    }
}
