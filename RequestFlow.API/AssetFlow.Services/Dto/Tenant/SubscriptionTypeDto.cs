namespace AssetFlow.Services.Dto.Tenant
{
    public class SubscriptionTypeDto : CreateSubscriptionTypeDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSubscriptionTypeDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double Order { get; set; }
        public string Description { get; set; }
    }

    public class UpdateSubscriptionTypeDto : CreateSubscriptionTypeDto
    {
        public int Id { get; set; }
    }

}
