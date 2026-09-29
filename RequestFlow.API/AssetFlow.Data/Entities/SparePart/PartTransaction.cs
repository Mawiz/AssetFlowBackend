using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;

namespace AssetFlow.Data.Entities.SparePart
{
    public class PartTransaction : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int PartId { get; set; }
        public virtual Part Part { get; set; }

        public int? PartSerialNumberId { get; set; }
        public virtual PartSerialNumber PartSerialNumber { get; set; }

        public int TransactionType { get; set; }

        public decimal Quantity { get; set; }

        public int? FromLocationId { get; set; }
        public virtual LocationEntity FromLocation { get; set; }

        public int? ToLocationId { get; set; }
        public virtual LocationEntity ToLocation { get; set; }

        public DateTime TransactionDate { get; set; }

        public int? PerformedByUserId { get; set; }
        public virtual ApplicationUser PerformedByUser { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; }
    }
}
