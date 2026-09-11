using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Tenant
{
    public class Language : BaseEnumModel
    {
        [StringLength(50)]
        public string Code { get; set; }  // e.g. "en", "ar"

        public virtual ICollection<TenantLanguage> TenantLanguages { get; set; } = new List<TenantLanguage>();
    }
}
