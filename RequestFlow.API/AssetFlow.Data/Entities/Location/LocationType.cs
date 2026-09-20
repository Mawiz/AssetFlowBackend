using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Location
{
    public class LocationType : BaseModel
    {
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        public int? ParentLocationTypeId { get; set; }
        public virtual LocationType ParentLocationType { get; set; }
        public virtual ICollection<LocationType> ChildLocationTypes { get; set; } = new List<LocationType>();

        [StringLength(500)]
        public string Description { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
