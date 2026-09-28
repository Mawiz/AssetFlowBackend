namespace AssetFlow.Services.Dto.Asset
{
    public class AssetCategoryDto : CreateAssetCategoryDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateAssetCategoryDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAssetCategoryDto : CreateAssetCategoryDto
    {
        public int Id { get; set; }
    }
}
