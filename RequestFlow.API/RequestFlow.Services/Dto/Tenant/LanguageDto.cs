using System.ComponentModel.DataAnnotations;

namespace RequestFlow.Services.Dto.Tenant
{
    public class CreateLanguageDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string DisplayName { get; set; }

        public double Order { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }
    }

    public class UpdateLanguageDto : CreateLanguageDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class LanguageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double Order { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}
