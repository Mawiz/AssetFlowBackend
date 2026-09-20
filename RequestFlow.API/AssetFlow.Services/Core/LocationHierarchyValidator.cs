using AssetFlow.Data.Entities.Location;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Services.Core
{
    public static class LocationHierarchyValidator
    {
        public static async Task<bool> WouldCreateLocationTypeCycleAsync(
            IQueryable<LocationType> types,
            int typeId,
            int? newParentTypeId)
        {
            if (!newParentTypeId.HasValue)
                return false;

            if (newParentTypeId.Value == typeId)
                return true;

            var parentMap = await types
                .Select(t => new { t.Id, t.ParentLocationTypeId })
                .ToListAsync();

            var visited = new HashSet<int>();
            var current = newParentTypeId.Value;

            while (true)
            {
                if (current == typeId)
                    return true;

                if (!visited.Add(current))
                    return true;

                var parent = parentMap.FirstOrDefault(x => x.Id == current);
                if (parent?.ParentLocationTypeId == null)
                    return false;

                current = parent.ParentLocationTypeId.Value;
            }
        }

        public static async Task<bool> WouldCreateLocationCycleAsync(
            IQueryable<Location> locations,
            int locationId,
            int? newParentLocationId)
        {
            if (!newParentLocationId.HasValue)
                return false;

            if (newParentLocationId.Value == locationId)
                return true;

            var parentMap = await locations
                .Select(l => new { l.Id, l.ParentLocationId })
                .ToListAsync();

            var visited = new HashSet<int>();
            var current = newParentLocationId.Value;

            while (true)
            {
                if (current == locationId)
                    return true;

                if (!visited.Add(current))
                    return true;

                var parent = parentMap.FirstOrDefault(x => x.Id == current);
                if (parent?.ParentLocationId == null)
                    return false;

                current = parent.ParentLocationId.Value;
            }
        }
    }
}
