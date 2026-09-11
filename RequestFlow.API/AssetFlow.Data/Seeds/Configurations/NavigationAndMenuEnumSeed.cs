using RequestFlow.Data.Entities.Configurations;
using Microsoft.EntityFrameworkCore;
using RequestFlow.Data.Entities.Configurations;

namespace RequestFlow.Data.Seeds.Configurations
{
    public class NavigationAndMenuEnumSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var dateTime = new DateTime(2024, 09, 17, 0, 0, 0);

            modelBuilder.Entity<NavigationItemEnum>()
                .HasData(new List<NavigationItemEnum>
                {
                 //new NavigationItemEnum { Id = 1, ApplicationRoleId = 1, Name = "Dashboard", DisplayName = "Dashboard", ModifiedOn = dateTime }
                });

            modelBuilder.Entity<NavigationCreateItemEnum>()
                .HasData(new List<NavigationCreateItemEnum>
                {
                //new NavigationCreateItemEnum { Id = 1, ApplicationRoleId = 1, Name = "Dashboard", DisplayName = "Dashboard", ModifiedOn = dateTime },
                });

            modelBuilder.Entity<MenuItemEnum>()
                .HasData(new List<MenuItemEnum>
                {
                //new MenuItemEnum { Id = 1, ApplicationRoleId = 1, Name = "Dashboard", DisplayName = "Dashboard", ModifiedOn = dateTime }
                });
        }
    }
}
