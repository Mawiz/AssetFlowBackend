namespace AssetFlow.Services.Dto.Asset
{
    public class AssetDto : CreateAssetDto
    {
        public int Id { get; set; }
        public string AssetCategoryName { get; set; }
        public string AssetTypeName { get; set; }
        public string LocationName { get; set; }
        public string ResponsibleUserName { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateAssetDto
    {
        public int? TenantId { get; set; }
        public string AssetCode { get; set; }
        public string Name { get; set; }
        public int AssetCategoryId { get; set; }
        public int AssetTypeId { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public DateTime? InstallationDate { get; set; }
        public int LocationId { get; set; }
        public int? ResponsibleUserId { get; set; }
        public string OtherLocationInformation { get; set; }
        public int Status { get; set; }
        public int Criticality { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string SupplierName { get; set; }
        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAssetDto : CreateAssetDto
    {
        public int Id { get; set; }
    }

    public class AssetFilterDto : SearchViewDto
    {
        public int? AssetCategoryId { get; set; }
        public int? AssetTypeId { get; set; }
        public int? LocationId { get; set; }
        public int? Status { get; set; }
        public int? Criticality { get; set; }
        public int? ResponsibleUserId { get; set; }
    }
}
