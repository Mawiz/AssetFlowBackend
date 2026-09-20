namespace AssetFlow.Services.Dto.Location
{
    public class LocationTypeDto : CreateLocationTypeDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string ParentLocationTypeName { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateLocationTypeDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public int? ParentLocationTypeId { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLocationTypeDto : CreateLocationTypeDto
    {
        public int Id { get; set; }
    }
}
