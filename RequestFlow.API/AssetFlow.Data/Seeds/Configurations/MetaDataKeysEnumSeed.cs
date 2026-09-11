using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Entities.Configurations;

namespace AssetFlow.Data.Seeds.Configurations
{
    public class MetaDataKeysEnumSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var dateTime = new DateTime(2024, 09, 17, 0, 0, 0);

            modelBuilder.Entity<MetaDataKeysEnum>()
                .HasData(new List<MetaDataKeysEnum>
                {
                new () { Id = 1, Name = "ApplicationRole", DisplayName = "ApplicationRole", ModifiedOn = dateTime }
                });
         }
    } 
}
