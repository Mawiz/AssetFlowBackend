namespace AssetFlow.Data.Entities.SparePart
{
    public class PartTransactionSerial : BaseModel
    {
        public int PartTransactionId { get; set; }
        public virtual PartTransaction PartTransaction { get; set; }

        public int PartSerialNumberId { get; set; }
        public virtual PartSerialNumber PartSerialNumber { get; set; }

        public decimal Quantity { get; set; } = 1;
    }
}
