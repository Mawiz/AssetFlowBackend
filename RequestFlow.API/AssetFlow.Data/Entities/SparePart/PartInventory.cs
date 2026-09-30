using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;

namespace AssetFlow.Data.Entities.SparePart
{
    public class PartInventory : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int PartId { get; set; }
        public virtual Part Part { get; set; }

        public int LocationId { get; set; }
        public virtual LocationEntity Location { get; set; }

        public int? PartSerialNumberId { get; set; }
        public virtual PartSerialNumber PartSerialNumber { get; set; }

        public int? PartTransactionId { get; set; }
        public virtual PartTransaction PartTransaction { get; set; }

        public decimal QuantityAvailable { get; set; }
        public decimal QuantityReserved { get; set; }

        public int Status { get; set; }
    }
}
