using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.SparePart
{
    public class PartDto : CreatePartDto
    {
        public int Id { get; set; }
        public string PartCategoryName { get; set; }
        public string TenantName { get; set; }
        public decimal CurrentStock { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class CreatePartDto
    {
        public int? TenantId { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public int PartCategoryId { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string SupplierName { get; set; }
        public string UnitOfMeasure { get; set; }
        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }
        public decimal MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePartDto : CreatePartDto
    {
        public int Id { get; set; }
    }

    public class PartFilterDto : SearchViewDto
    {
        public int? PartCategoryId { get; set; }
        public bool? IsSerialized { get; set; }
        public bool? LowStockOnly { get; set; }
    }
}
