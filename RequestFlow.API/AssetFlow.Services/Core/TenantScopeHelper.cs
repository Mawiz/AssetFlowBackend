using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Provider;

namespace AssetFlow.Services.Core
{
    public static class TenantScopeHelper
    {
        public static int? GetContextTenantId(ITenantProvider tenantProvider)
        {
            var id = tenantProvider.GetTenantId();
            return id == 0 ? null : id;
        }

        public static bool IsSystemAdmin(ITenantProvider tenantProvider)
            => !GetContextTenantId(tenantProvider).HasValue;

        public static (bool Ok, int? TenantId, string Error) ResolveWriteTenantId(
            ITenantProvider tenantProvider,
            int? dtoTenantId)
        {
            var contextTenantId = GetContextTenantId(tenantProvider);
            if (contextTenantId.HasValue)
                return (true, contextTenantId, string.Empty);

            if (!dtoTenantId.HasValue || dtoTenantId == 0)
                return (false, null, "Tenant is required.");

            return (true, dtoTenantId, string.Empty);
        }

        public static IQueryable<T> ApplyAdminListTenantFilter<T>(
            IQueryable<T> query,
            ITenantProvider tenantProvider,
            int? filterTenantId) where T : class, ITenancyModel
        {
            if (!IsSystemAdmin(tenantProvider))
                return query;

            if (filterTenantId.HasValue)
                return query.Where(x => x.TenantId == filterTenantId);

            return query;
        }
    }
}
