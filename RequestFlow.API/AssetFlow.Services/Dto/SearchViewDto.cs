using AssetFlow.Common;
using AssetFlow.Common.Enum;

namespace AssetFlow.Services.Dto
{
    public class SearchViewDto
    {
        public int PageSize { get; set; } = Convert.ToInt32(AppResource.PageSize);
        public int PageNumber { get; set; } = Convert.ToInt32(AppResource.PageNumber);
        public string? SearchText { get; set; }
        public string? OrderByProp { get; set; }
        public int? SortDirection { get; set; } = (int)Enums.OrderBy.Ascending;
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
