using AssetFlow.Data.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AssetFlow.Data.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ASSETFLOW_CONNECTION")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AssetFlow;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ApplicationDbContext(options, new NullHttpContextAccessor(), new DesignTimeTenantProvider());
        }

        private sealed class NullHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
        }

        private sealed class DesignTimeTenantProvider : ITenantProvider
        {
            public int? GetTenantId() => null;
        }
    }
}
