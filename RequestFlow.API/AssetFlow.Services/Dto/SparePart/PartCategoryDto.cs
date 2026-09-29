namespace AssetFlow.Services.Dto.SparePart
{
    public class PartCategoryDto : CreatePartCategoryDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
    }

    public class CreatePartCategoryDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePartCategoryDto : CreatePartCategoryDto
    {
        public int Id { get; set; }
    }
}
