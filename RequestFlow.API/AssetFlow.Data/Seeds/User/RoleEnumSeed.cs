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

            //modelBuilder.Entity<ApplicationRole>()
            //    .HasData(new List<ApplicationRole>
            //    {
            //        new ApplicationRole
            //        {
            //            Id = 1,
            //            Order = 1,
            //            Name = "Admin",
            //            NormalizedName = "ADMIN",
            //            DisplayName = "Admin",
            //            Description = "Admin",
            //            ConcurrencyStamp = string.Empty,
            //            CreatedOn = dateTime,
            //            ModifiedOn = dateTime
            //        },
            //        new ApplicationRole
            //        {
            //            Id = 2,
            //            Order = 2,
            //            Name = "Student",
            //            NormalizedName = "STUDENT",
            //            DisplayName = "Student",
            //            Description = "Student",
            //            ConcurrencyStamp = string.Empty,
            //            CreatedOn = dateTime,
            //            ModifiedOn = dateTime
            //        }
            //    });

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
                    new Resource { Id = userFeatureId, ResourceName = "User", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = roleFeatureId, ResourceName = "Role", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = resourceFeatureId, ResourceName = "Resource", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = tenantFeatureId, ResourceName = "Tenant", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = subscriptionFeatureId, ResourceName = "Subscription", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = languageFeatureId, ResourceName = "Language", Verb = string.Empty, IsBackEnd = false },
                    new Resource { Id = metaDataFeatureId, ResourceName = "MetaData", Verb = string.Empty, IsBackEnd = false },

                    new Resource { Id = 8, FeatureId = userFeatureId, ResourceName = Permissions.UserCreate, Verb = "post/api/User", IsBackEnd = true },
                    new Resource { Id = 9, FeatureId = userFeatureId, ResourceName = Permissions.UserUpdate, Verb = "put/api/User", IsBackEnd = true },
                    new Resource { Id = 10, FeatureId = userFeatureId, ResourceName = Permissions.UserView, Verb = "get/api/User/{id}", IsBackEnd = true },
                    new Resource { Id = 11, FeatureId = userFeatureId, ResourceName = Permissions.UserList, Verb = "post/api/User/Filter", IsBackEnd = true },
                    new Resource { Id = 12, FeatureId = userFeatureId, ResourceName = Permissions.UserToggle, Verb = "put/api/User/ToggleActive", IsBackEnd = true },

                    new Resource { Id = 13, FeatureId = roleFeatureId, ResourceName = Permissions.RoleCreate, Verb = "post/api/ApplicationRole", IsBackEnd = true },
                    new Resource { Id = 14, FeatureId = roleFeatureId, ResourceName = Permissions.RoleUpdate, Verb = "put/api/ApplicationRole", IsBackEnd = true },
                    new Resource { Id = 15, FeatureId = roleFeatureId, ResourceName = Permissions.RoleView, Verb = "get/api/ApplicationRole", IsBackEnd = true },
                    new Resource { Id = 16, FeatureId = roleFeatureId, ResourceName = Permissions.RoleList, Verb = "post/api/ApplicationRole/Filter", IsBackEnd = true },

                    new Resource { Id = 17, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceCreate, Verb = "post/api/Resource", IsBackEnd = true },
                    new Resource { Id = 18, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceUpdate, Verb = "put/api/Resource", IsBackEnd = true },
                    new Resource { Id = 19, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceView, Verb = "get/api/Resource", IsBackEnd = true },
                    new Resource { Id = 20, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceList, Verb = "post/api/Resource/Filter", IsBackEnd = true },

                    new Resource { Id = 21, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantCreate, Verb = "post/api/Tenant", IsBackEnd = true },
                    new Resource { Id = 22, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantUpdate, Verb = "put/api/Tenant", IsBackEnd = true },
                    new Resource { Id = 23, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantView, Verb = "get/api/Tenant/{id}", IsBackEnd = true },
                    new Resource { Id = 24, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantList, Verb = "post/api/Tenant/Filter", IsBackEnd = true },
                    new Resource { Id = 25, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantToggle, Verb = "patch/api/Tenant/{id}/toggle-status", IsBackEnd = true },

                    new Resource { Id = 26, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionCreate, Verb = "post/api/Subscription", IsBackEnd = true },
                    new Resource { Id = 27, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionUpdate, Verb = "put/api/Subscription", IsBackEnd = true },
                    new Resource { Id = 28, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionView, Verb = "get/api/Subscription/{id}", IsBackEnd = true },
                    new Resource { Id = 29, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionList, Verb = "post/api/Subscription/Filter", IsBackEnd = true },
                    new Resource { Id = 30, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionToggle, Verb = "patch/api/Subscription/{id}/toggle-status", IsBackEnd = true },
                    new Resource { Id = 31, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionDelete, Verb = "delete/api/Subscription/{id}", IsBackEnd = true },

                    new Resource { Id = 32, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageCreate, Verb = "post/api/Language/create", IsBackEnd = true },
                    new Resource { Id = 33, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageUpdate, Verb = "put/api/Language/update", IsBackEnd = true },
                    new Resource { Id = 34, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageView, Verb = "get/api/Language/{id}", IsBackEnd = true },
                    new Resource { Id = 35, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageList, Verb = "get/api/Language/list", IsBackEnd = true },
                    new Resource { Id = 36, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageToggle, Verb = "patch/api/Language/toggle-status/{id}", IsBackEnd = true },

                    new Resource { Id = 37, FeatureId = metaDataFeatureId, ResourceName = Permissions.MetaDataView, Verb = "post/api/MetaData/GetMetaDataValues", IsBackEnd = true }
                });

            var adminRoleResources = Enumerable.Range(8, 30)
                .Select((resourceId, index) => new RoleResource
                {
                    Id = index + 1,
                    ApplicationRoleId = 1,
                    ResourceId = resourceId
                })
                .ToList();

            modelBuilder.Entity<RoleResource>().HasData(adminRoleResources);
        }
    }
}
