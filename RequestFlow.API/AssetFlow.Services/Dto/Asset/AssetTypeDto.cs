using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.Asset
{
    public class AssetTypeDto : CreateAssetTypeDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string AssetCategoryName { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateAssetTypeDto
    {
        public int? TenantId { get; set; }
        public int AssetCategoryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAssetTypeDto : CreateAssetTypeDto
    {
        public int Id { get; set; }
    }

    public class AssetTypeFilterDto : SearchViewDto
    {
        public int? AssetCategoryId { get; set; }
    }
}
