using AssetFlow.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Services.Core
{
    public static class PartSerialNumberGenerator
    {
        /// <summary>
        /// Next serial for a part: {yyyy}-{NNNN} (e.g. 2026-0001), sequence per part and year within tenant.
        /// </summary>
        public static async Task<string> GetNextSerialAsync(ApplicationDbContext context, int partId, int? tenantId)
        {
            var part = await context.Parts.FindAsync(partId);
            if (part == null)
                throw new InvalidOperationException("Part not found.");

            var year = DateTime.UtcNow.Year;
            var prefix = $"{year}-";

            var existing = await context.PartSerialNumbers
                .Where(x => x.TenantId == tenantId && x.PartId == partId && x.SerialNumber.StartsWith(prefix))
                .Select(x => x.SerialNumber)
                .ToListAsync();

            var max = 0;
            foreach (var s in existing)
            {
                if (s.Length <= prefix.Length) continue;
                var suffix = s.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n))
                    max = Math.Max(max, n);
            }

            return prefix + (max + 1).ToString("D4");
        }

        public static async Task<List<string>> GetNextSerialsAsync(ApplicationDbContext context, int partId, int? tenantId, int count)
        {
            if (count <= 0)
                return new List<string>();

            var part = await context.Parts.FindAsync(partId);
            if (part == null)
                throw new InvalidOperationException("Part not found.");

            var year = DateTime.UtcNow.Year;
            var prefix = $"{year}-";

            var existing = await context.PartSerialNumbers
                .Where(x => x.TenantId == tenantId && x.PartId == partId && x.SerialNumber.StartsWith(prefix))
                .Select(x => x.SerialNumber)
                .ToListAsync();

            var max = 0;
            foreach (var s in existing)
            {
                if (s.Length <= prefix.Length) continue;
                var suffix = s.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n))
                    max = Math.Max(max, n);
            }

            return Enumerable.Range(max + 1, count)
                .Select(n => prefix + n.ToString("D4"))
                .ToList();
        }

        public static async Task<bool> SerialExistsAsync(ApplicationDbContext context, int? tenantId, string serial, int? excludeSerialId = null)
        {
            if (string.IsNullOrWhiteSpace(serial)) return false;
            var normalized = serial.Trim();
            var query = context.PartSerialNumbers.Where(x =>
                x.TenantId == tenantId &&
                x.SerialNumber.ToLower() == normalized.ToLower());

            if (excludeSerialId.HasValue)
                query = query.Where(x => x.Id != excludeSerialId.Value);

            return await query.AnyAsync();
        }
    }
}
