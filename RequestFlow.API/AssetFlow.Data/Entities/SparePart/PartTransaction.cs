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



        public int? PartInventoryId { get; set; }

        public virtual PartInventory PartInventory { get; set; }



        public int? PartInventoryBatchId { get; set; }

        public virtual PartInventoryBatch PartInventoryBatch { get; set; }



        public int TransactionType { get; set; }



        public decimal Quantity { get; set; }



        public int? FromLocationId { get; set; }

        public virtual LocationEntity FromLocation { get; set; }



        public int? ToLocationId { get; set; }

        public virtual LocationEntity ToLocation { get; set; }



        public DateTime TransactionDate { get; set; }



        public int? PerformedByUserId { get; set; }

        public virtual ApplicationUser PerformedByUser { get; set; }



        public int? IssuedToUserId { get; set; }

        public virtual ApplicationUser IssuedToUser { get; set; }



        public int? ReturnedFromUserId { get; set; }

        public virtual ApplicationUser ReturnedFromUser { get; set; }



        [StringLength(500)]

        public string Reason { get; set; }



        [StringLength(1000)]

        public string Remarks { get; set; }



        public int? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }



        public virtual ICollection<PartTransactionSerial> TransactionSerials { get; set; } = new List<PartTransactionSerial>();

    }

}


