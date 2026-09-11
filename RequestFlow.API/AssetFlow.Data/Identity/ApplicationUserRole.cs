using Microsoft.AspNetCore.Identity;

namespace AssetFlow.Data.Identity
{
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public ApplicationRole Role { get; set; }
        public ApplicationUser User { get; set; }
    }
}
