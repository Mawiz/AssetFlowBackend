namespace AssetFlow.Services.Dto.Tenant
{
    public class CreateTenantDto
    {
        public string CompanyName { get; set; }
        public int SubscriptionTypeId { get; set; }
        public List<int> LanguageIds { get; set; } = new();
    }

    public class UpdateTenantDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int SubscriptionTypeId { get; set; }
        public List<int> LanguageIds { get; set; } = new();
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int SubscriptionTypeId { get; set; }
        public string   SubscriptionName { get; set; }
        public List<int> LanguageIds { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
