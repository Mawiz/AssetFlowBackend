using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.Location
{
    public class LocationDto : CreateLocationDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string LocationTypeName { get; set; }
        public string ParentLocationName { get; set; }
    }

    public class CreateLocationDto
    {
        public int LocationTypeId { get; set; }
        public int? ParentLocationId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLocationDto : CreateLocationDto
    {
        public int Id { get; set; }
    }

    public class LocationFilterDto : SearchViewDto
    {
        public int? LocationTypeId { get; set; }
    }
}
