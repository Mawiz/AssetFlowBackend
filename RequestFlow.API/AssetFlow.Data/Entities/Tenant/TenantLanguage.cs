using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFlow.Data.Entities.Tenant
{
    public class TenantLanguage : BaseIdModel
    {
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
         
        public int LanguageId { get; set; }
        public virtual Language Language { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
