namespace RequestFlow.Services.Dto.MetaData
{
    public class MetaDataRequestDto
    {
        public List<string> SecretKeys { get; set; } = new();
        public DateTime? LatestByDate { get; set; }  
    }
}
