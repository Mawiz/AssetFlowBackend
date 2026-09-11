using System.Collections.Generic;

namespace AssetFlow.Services.Dto.User
{
    public class LoginUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public int? InventoryOwnerEnumId { get; set; }
        public List<int> FieldUnits { get; set; } = new();
        public int? PageSize { get; set; }
    }
}
