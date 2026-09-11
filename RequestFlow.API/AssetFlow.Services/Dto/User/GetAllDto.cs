using System;
using System.Collections.Generic;

namespace AssetFlow.Services.Dto.User
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public char FirstLetter { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string RoleName { get; set; }
        public string TenantName { get; set; }
        public int? TenantId { get; set; }
    }
    public class ListGetUserDto
    {
        public char FirstLetter { get; set; }
        public List<GetUserDto> Data { get; set; }
    }
}
