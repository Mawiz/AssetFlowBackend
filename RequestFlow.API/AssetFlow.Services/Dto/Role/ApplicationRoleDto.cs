using AssetFlow.Services.Dto.Role.RoleResource;

namespace AssetFlow.Services.Dto.Role
{
    //public class ApplicationRoleDto
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }           // IdentityRole.Name
    //    public string DisplayName { get; set; }
    //    public string Description { get; set; }
    //    public double Order { get; set; }
    //    public bool IsActive { get; set; } = true;
    //}

    //public class CreateApplicationRoleDto
    //{
    //    public string Name { get; set; }
    //    public string DisplayName { get; set; }
    //    public string Description { get; set; }
    //    public double Order { get; set; }
    //    public int? TenantId { get; set; }
    //}

    //public class UpdateApplicationRoleDto : CreateApplicationRoleDto
    //{
    //    public int Id { get; set; }
    //}
    public class CreateRoleDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public List<int> ResourceIds { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<int> ResourceIds { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
    }

    public class UpdateRoleDto
    {
        public int Id { get; set; } // Role ID
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<int> ResourceIds { get; set; } = new();
        public int? TenantId { get; set; }
    }

    public class RoleWithResourcesDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }

        public List<ResourceDto> Resources { get; set; } = new();
    }
}
