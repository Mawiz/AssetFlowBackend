namespace AssetFlow.Services.Dto.Asset
{
    public class AssetComponentDto : CreateAssetComponentDto
    {
        public int Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateAssetComponentDto
    {
        public int? TenantId { get; set; }
        public int AssetId { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string PartNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? InstallationDate { get; set; }
        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }
        public int CurrentStatus { get; set; }
        public string SupplierName { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string InstallationLocation { get; set; }
        public decimal? CurrentRunningHours { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAssetComponentDto : CreateAssetComponentDto
    {
        public int Id { get; set; }
    }

    public class AssetComponentFilterDto : SearchViewDto
    {
        public int? AssetId { get; set; }
    }
}
