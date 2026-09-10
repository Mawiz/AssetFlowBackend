using Microsoft.AspNetCore.Identity;

namespace RequestFlow.Data.Identity
{
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public ApplicationRole Role { get; set; }
        public ApplicationUser User { get; set; }
    }
}
