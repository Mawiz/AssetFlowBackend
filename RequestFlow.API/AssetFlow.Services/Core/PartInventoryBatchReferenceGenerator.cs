using AssetFlow.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Services.Core
{
    public static class PartInventoryBatchReferenceGenerator
    {
        public static async Task<string> GetNextReferenceAsync(ApplicationDbContext context, int? tenantId)
        {
            var prefix = "B-";
            var existing = await context.PartInventoryBatches
                .Where(x => x.TenantId == tenantId && x.BatchReference.StartsWith(prefix))
                .Select(x => x.BatchReference)
                .ToListAsync();

            var max = 0;
            foreach (var s in existing)
            {
                if (s.Length <= prefix.Length) continue;
                var suffix = s.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n))
                    max = Math.Max(max, n);
            }

            return prefix + (max + 1).ToString("D3");
        }
    }
}
