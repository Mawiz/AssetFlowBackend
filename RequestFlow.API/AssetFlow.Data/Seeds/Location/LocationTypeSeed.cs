using AssetFlow.Data.Entities.Location;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Seeds.Location
{
    public static class LocationTypeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var createdOn = new DateTime(2021, 11, 26, 0, 0, 0);

            modelBuilder.Entity<LocationType>().HasData(
                new LocationType
                {
                    Id = 1,
                    Name = "Company",
                    Code = "COMPANY",
                    ParentLocationTypeId = null,
                    Description = "Company",
                    SortOrder = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                },
                new LocationType
                {
                    Id = 2,
                    Name = "Factory",
                    Code = "FACTORY",
                    ParentLocationTypeId = 1,
                    Description = "Factory",
                    SortOrder = 2,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                },
                new LocationType
                {
                    Id = 3,
                    Name = "Building",
                    Code = "BUILDING",
                    ParentLocationTypeId = 2,
                    Description = "Building",
                    SortOrder = 3,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                },
                new LocationType
                {
                    Id = 4,
                    Name = "Floor",
                    Code = "FLOOR",
                    ParentLocationTypeId = 3,
                    Description = "Floor",
                    SortOrder = 4,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                },
                new LocationType
                {
                    Id = 5,
                    Name = "Production Area",
                    Code = "PRODUCTION_AREA",
                    ParentLocationTypeId = 4,
                    Description = "Production Area",
                    SortOrder = 5,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                },
                new LocationType
                {
                    Id = 6,
                    Name = "Room",
                    Code = "ROOM",
                    ParentLocationTypeId = 5,
                    Description = "Room",
                    SortOrder = 6,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn
                });
        }
    }
}
