using AssetFlow.Data.Entities.Tenant;

using LocationEntity = AssetFlow.Data.Entities.Location.Location;



namespace AssetFlow.Data.Entities.SparePart

{

    /// <summary>Aggregate stock for Part + Location.</summary>

    public class PartInventory : BaseModel, ITenancyModel

    {

        public int? TenantId { get; set; }

        public virtual Tenant.Tenant Tenant { get; set; }



        public int PartId { get; set; }

        public virtual Part Part { get; set; }



        public int LocationId { get; set; }

        public virtual LocationEntity Location { get; set; }



        public decimal TotalQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal FaultyQuantity { get; set; }

        public decimal QuarantineQuantity { get; set; }

        public decimal IssuedQuantity { get; set; }



        public virtual ICollection<PartInventoryBatch> Batches { get; set; } = new List<PartInventoryBatch>();

    }

}


