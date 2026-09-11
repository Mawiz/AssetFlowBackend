using AssetFlow.Data.Identity;

namespace AssetFlow.Data.Entities
{
    public class BaseModel : BaseIdModel
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
        public int? ModifiedById { get; set; }
        public ApplicationUser ModifiedBy { get; set; }
   
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
    }
}
