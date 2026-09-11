namespace AssetFlow.Services.Dto.MetaData
{
    public class MetaDataResponseDto
    {
        public string Key { get; set; }
        public object Data { get; set; }
    }
    public class ListMetaDataResponseDto
    {
        public List<MetaDataResponseDto> MetaResult { get; set; } = new ();
        public DateTime? ModifiedOn { get; set; }
    }
}
