using System;
using System.Collections.Generic;

namespace AssetFlow.Services.Dto.User
{
    public class UserByRoleRequestDto
    {
        public List<int> Roles { get; set; } = new();
        public bool? IsActive { get; set; }
        public DateTime? LatestByDate { get; set; }
    }
}
