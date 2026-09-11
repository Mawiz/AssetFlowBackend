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

            modelBuilder.Entity<ApplicationRole>()
                .HasData(new List<ApplicationRole>
                {
                new ApplicationRole { Id = 1, Order = 2, Name = "Admin", DisplayName = "Admin" , Description = "Admin", ConcurrencyStamp = string.Empty, ModifiedOn = dateTime },
                new ApplicationRole { Id = 2, Order = 3, Name = "Student", DisplayName = "Student", Description = "Student", ConcurrencyStamp = string.Empty, ModifiedOn = dateTime },
                  });

            modelBuilder.Entity<Resource>()
                .HasData(new List<Resource>
                {

                #region UserManagement

                new Resource { Id = 1, ResourceName = "CreateUser", Verb = "post/api/User",  IsBackEnd = true },
                new Resource { Id = 2, ResourceName = "UpdateUser", Verb = "put/api/User",  IsBackEnd = true },
                new Resource { Id = 3, ResourceName = "GetUserById", Verb = "get/api/User/{id}",  IsBackEnd = true },
                new Resource { Id = 4, ResourceName = "UserFilter", Verb = "post/api/User/Filter",  IsBackEnd = true },
                new Resource { Id = 5, ResourceName = "UserToggle", Verb = "put/api/User/ToggleActive",  IsBackEnd = true },
                
                #endregion UserManagement

                #region MetaDataAndEnums
                
                new Resource { Id = 6, ResourceName = "MetaData", Verb = "post/api/MetaData/GetMetaDataValues",  IsBackEnd = true },
                new Resource { Id = 7, ResourceName = "Enums", Verb = "get/api/MetaData/GetMetaDataEnums",  IsBackEnd = true },

                #endregion MetaDataAndEnums           
                });


            modelBuilder.Entity<RoleResource>()
                .HasData(new List<RoleResource>
                {

                #region UserManagement

                //Admin & Office staff
                new RoleResource { Id = 1, ApplicationRoleId = 1, ResourceId = 1 },
                new RoleResource { Id = 2, ApplicationRoleId = 1, ResourceId = 2 },
                new RoleResource { Id = 3, ApplicationRoleId = 1, ResourceId = 3 },
                new RoleResource { Id = 4, ApplicationRoleId = 1, ResourceId = 4 },
                new RoleResource { Id = 5, ApplicationRoleId = 1, ResourceId = 5 },

                new RoleResource {Id = 6, ApplicationRoleId = 2, ResourceId = 1 },
                new RoleResource {Id = 7, ApplicationRoleId = 2, ResourceId = 2 },
                new RoleResource {Id = 8, ApplicationRoleId = 2, ResourceId = 3 },
                new RoleResource {Id = 9, ApplicationRoleId = 2, ResourceId = 4 },
                new RoleResource {Id = 10, ApplicationRoleId = 2, ResourceId = 5 },

                #endregion UserManagement

                #region MetaDataAndEnums

                //All Users
                new RoleResource {Id = 11, ApplicationRoleId = 1, ResourceId = 6 },
                new RoleResource {Id = 12, ApplicationRoleId = 2, ResourceId = 6 },

                new RoleResource {Id = 13, ApplicationRoleId = 1, ResourceId = 7 },
                new RoleResource {Id = 14, ApplicationRoleId = 2, ResourceId = 7 },

                #endregion MetaDataAndEnums
                });
        }
    }
}
