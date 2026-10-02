using AssetFlow.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Services.Core.Issue
{
    internal static class IssueLocationPathHelper
    {
        public static async Task<string> BuildPathAsync(ApplicationDbContext context, int locationId)
        {
            var names = new List<string>();
            var currentId = (int?)locationId;
            var guard = 0;
            while (currentId.HasValue && guard++ < 20)
            {
                var loc = await context.Locations.AsNoTracking()
                    .Where(x => x.Id == currentId.Value)
                    .Select(x => new { x.Name, x.ParentLocationId })
                    .FirstOrDefaultAsync();
                if (loc == null) break;
                names.Insert(0, loc.Name);
                currentId = loc.ParentLocationId;
            }
            return names.Count == 0 ? string.Empty : string.Join(" → ", names);
        }
    }
}
