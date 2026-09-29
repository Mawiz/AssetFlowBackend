namespace AssetFlow.Services.Dto.SparePart
{
    public class SupplierDto : CreateSupplierDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
    }

    public class CreateSupplierDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        public int Id { get; set; }
    }
}
