using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Entities.ACL
{
    public abstract class ACLContext : IdentityDbContext
         <ApplicationUser,
         ApplicationRole,
         int,
         IdentityUserClaim<int>,
         ApplicationUserRole,
         IdentityUserLogin<int>,
         IdentityRoleClaim<int>,
         IdentityUserToken<int>>
    {
        public ACLContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<RoleResource> RoleResources { get; set; }
    }
}
