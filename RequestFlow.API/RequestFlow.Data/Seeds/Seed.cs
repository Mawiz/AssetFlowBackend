using RequestFlow.Data.Seeds.Configurations;
using RequestFlow.Data.Seeds.User;
using Microsoft.EntityFrameworkCore;

namespace RequestFlow.Data.Seeds
{
    public class Seed
    {
        public static void Run(ModelBuilder modelBuilder)
        {
            RoleEnumSeed.Seed(modelBuilder);
            NavigationAndMenuEnumSeed.Seed(modelBuilder);
            MetaDataKeysEnumSeed.Seed(modelBuilder);
        }
    }
}
