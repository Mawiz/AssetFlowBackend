namespace AssetFlow.Services.Dto.MetaData
{
    public class MetaDataByTypeItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool HasChildren { get; set; }
        public int? ParentId { get; set; }
        public int? LocationTypeId { get; set; }
        public int? TenantId { get; set; }

        /// <summary>e.g. part number on asset components.</summary>
        public string Code { get; set; }

        public bool? IsSerialized { get; set; }

        public decimal? AvailableQuantity { get; set; }
    }
}
