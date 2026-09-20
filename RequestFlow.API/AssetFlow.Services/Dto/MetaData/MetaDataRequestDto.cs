namespace AssetFlow.Services.Dto.MetaData
{
    public class MetaDataRequestDto
    {
        public List<string> SecretKeys { get; set; } = new();
        public DateTime? LatestByDate { get; set; }
        /// <summary>Optional. When loading ApplicationRole: 0 = system roles only; otherwise filter by tenant id.</summary>
        public int? TenantId { get; set; }
    }
}
