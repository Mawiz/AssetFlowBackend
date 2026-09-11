using System.Collections.Generic;

namespace AssetFlow.Services.Dto.User
{
    public class FilterDto:SearchViewDto
    {
        public List<int> Roles { get; set; } = new();
    }
}
