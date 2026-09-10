using System.Collections.Generic;

namespace RequestFlow.Services.Dto.User
{
    public class FilterDto:SearchViewDto
    {
        public List<int> Roles { get; set; } = new();
    }
}
