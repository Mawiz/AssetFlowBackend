namespace AssetFlow.Services.Dto.MetaData
{
    public class MetaDataByTypeRequestDto
    {
        /// <summary>Entity type name, e.g. Location, LocationType, AssetCategory, AssetType, ApplicationUser, Department.</summary>
        public string Type { get; set; }

        /// <summary>When set, returns direct children of this parent (e.g. child locations, child location types, asset types for category).</summary>
        public int? ParentId { get; set; }

        public int? TenantId { get; set; }

        /// <summary>Optional filter when Type is Location — restrict to a location type level.</summary>
        public int? LocationTypeId { get; set; }
    }
}
