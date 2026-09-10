namespace RequestFlow.Common.Settings
{
    public class SystemSettings
    {
        public bool ShowExMsg { get; set; }
        public string MediaStorageFolder { get; set; }
        public string TempMediaStorageFolder { get; set; }

        public int? PageSize { get; set; }
        public int ClosedJobsDaysLimit { get; set; }
        public List<int> ExcludedJobs { get; set; } = new();
    }
}
