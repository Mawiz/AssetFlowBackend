namespace AssetFlow.Services.Dto.Role.RoleResource
{
    public class CreateResourceDto
    {
        public string ResourceName { get; set; }
        public string Verb { get; set; } = string.Empty;
        public bool IsBackEnd { get; set; }
        public List<CreateSubResourceDto> SubResources { get; set; }
    }

    public class CreateSubResourceDto
    {
        public string ResourceName { get; set; }
        public string Verb { get; set; } = string.Empty;
        public bool IsBackEnd { get; set; }
    }

    public class UpdateResourceDto
    {
        public int Id { get; set; }
        public string ResourceName { get; set; }
        public string Verb { get; set; } = string.Empty;
        public bool IsBackEnd { get; set; }
        public List<UpdateSubResourceDto> SubResources { get; set; }
    }

    public class UpdateSubResourceDto
    {
        public int Id { get; set; }
        public string ResourceName { get; set; }
        public string Verb { get; set; } = string.Empty;
        public bool IsBackEnd { get; set; }
    }

    public class ResourceDto
    {
        public int Id { get; set; }
        public string ResourceName { get; set; }
        public List<SubResourceDto> SubResources { get; set; }
    }

    public class SubResourceDto
    {
        public int Id { get; set; }
        public string ResourceName { get; set; }
    }
}
