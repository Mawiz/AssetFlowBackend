using Microsoft.AspNetCore.Http;

namespace AssetFlow.Data.Provider
{
    public interface ITenantProvider
    {
        int? GetTenantId();
    }

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantProvider(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;
        public int? GetTenantId()
        {

            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
            {
                var tenantId = _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == "tenantId").FirstOrDefault()?.Value;
                return string.IsNullOrEmpty(tenantId) || tenantId == "0" ? null : int.Parse(tenantId);
            }
            return null;
        }
    }
}
