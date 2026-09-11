using System.ComponentModel.DataAnnotations;

namespace RequestFlow.Data.Entities
{
    public class BaseEnumModel : BaseModel
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string DisplayName { get; set; }
        public double Order { get; set; }
    }
}
