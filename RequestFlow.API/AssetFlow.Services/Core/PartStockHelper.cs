using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;

namespace AssetFlow.Services.Core
{
    public static class PartStockHelper
    {
        public static async Task<decimal> GetAvailableStockAsync(ApplicationDbContext context, int partId)
        {
            return await context.PartInventories
                .Where(i => i.PartId == partId && i.IsActive && !i.IsDeleted)
                .SumAsync(i => i.AvailableQuantity);
        }
    }
}
