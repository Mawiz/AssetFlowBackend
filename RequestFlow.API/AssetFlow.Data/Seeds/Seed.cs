using AssetFlow.Data.Seeds.Configurations;
using AssetFlow.Data.Seeds.Location;
using AssetFlow.Data.Seeds.User;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Seeds
{
    public class Seed
    {
        public static void Run(ModelBuilder modelBuilder)
        {
            RoleEnumSeed.Seed(modelBuilder);
            //LocationTypeSeed.Seed(modelBuilder);
            //NavigationAndMenuEnumSeed.Seed(modelBuilder);
            //MetaDataKeysEnumSeed.Seed(modelBuilder);
        }
    }
}
