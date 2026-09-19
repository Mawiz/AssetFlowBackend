using AssetFlow.Common.Helper;

using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Seeds.User
{

    public static class RoleEnumSeed
    {

        public static void Seed(ModelBuilder modelBuilder)
        {

            var dateTime = new DateTime(2021, 11, 26, 0, 0, 0);

            const int userFeatureId = 1;

            const int roleFeatureId = 2;

            const int resourceFeatureId = 3;

            const int tenantFeatureId = 4;

            const int subscriptionFeatureId = 5;

            const int languageFeatureId = 6;

            const int metaDataFeatureId = 7;

            modelBuilder.Entity<Resource>()

                .HasData(new List<Resource>

                {

                    new Resource { Id = userFeatureId, ResourceName = "User" },

                    new Resource { Id = roleFeatureId, ResourceName = "Role" },

                    new Resource { Id = resourceFeatureId, ResourceName = "Resource" },

                    new Resource { Id = tenantFeatureId, ResourceName = "Tenant" },

                    new Resource { Id = subscriptionFeatureId, ResourceName = "Subscription" },

                    new Resource { Id = languageFeatureId, ResourceName = "Language" },

                    new Resource { Id = metaDataFeatureId, ResourceName = "MetaData" },



                    new Resource { Id = 8, FeatureId = userFeatureId, ResourceName = Permissions.UserCreate },

                    new Resource { Id = 9, FeatureId = userFeatureId, ResourceName = Permissions.UserUpdate },

                    new Resource { Id = 10, FeatureId = userFeatureId, ResourceName = Permissions.UserView },

                    new Resource { Id = 11, FeatureId = userFeatureId, ResourceName = Permissions.UserList },

                    new Resource { Id = 12, FeatureId = userFeatureId, ResourceName = Permissions.UserToggle },



                    new Resource { Id = 13, FeatureId = roleFeatureId, ResourceName = Permissions.RoleCreate },

                    new Resource { Id = 14, FeatureId = roleFeatureId, ResourceName = Permissions.RoleUpdate },

                    new Resource { Id = 15, FeatureId = roleFeatureId, ResourceName = Permissions.RoleView },

                    new Resource { Id = 16, FeatureId = roleFeatureId, ResourceName = Permissions.RoleList },



                    new Resource { Id = 17, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceCreate },

                    new Resource { Id = 18, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceUpdate },

                    new Resource { Id = 19, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceView },

                    new Resource { Id = 20, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceList },



                    new Resource { Id = 21, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantCreate },

                    new Resource { Id = 22, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantUpdate },

                    new Resource { Id = 23, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantView },

                    new Resource { Id = 24, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantList },

                    new Resource { Id = 25, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantToggle },



                    new Resource { Id = 26, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionCreate },

                    new Resource { Id = 27, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionUpdate },

                    new Resource { Id = 28, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionView },

                    new Resource { Id = 29, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionList },

                    new Resource { Id = 30, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionToggle },

                    new Resource { Id = 31, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionDelete },



                    new Resource { Id = 32, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageCreate },

                    new Resource { Id = 33, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageUpdate },

                    new Resource { Id = 34, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageView },

                    new Resource { Id = 35, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageList },

                    new Resource { Id = 36, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageToggle },



                    new Resource { Id = 37, FeatureId = metaDataFeatureId, ResourceName = Permissions.MetaDataView }

                });

            //modelBuilder.Entity<ApplicationRole>()
            //    .HasData(new List<ApplicationRole>
            //    {
            //        new ApplicationRole
            //        {
            //            Id = 1,
            //            Order = 1,
            //            Name = "CEO Manufactureur",
            //            NormalizedName = "CEO",
            //            DisplayName = "CEO",
            //            Description = "CEO ",
            //            ConcurrencyStamp = string.Empty,
            //            CreatedOn = dateTime,
            //            ModifiedOn = dateTime
            //        }
            //    });

            //var adminRoleResources = Enumerable.Range(8, 30)

            //    .Select((resourceId, index) => new RoleResource

            //    {

            //        Id = index + 1,

            //        ApplicationRoleId = 1,

            //        ResourceId = resourceId

            //    })

            //    .ToList();



            //modelBuilder.Entity<RoleResource>().HasData(adminRoleResources);

        }

    }

}


