using AssetFlow.Data.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Identity
{
    public class TenantScopedRoleValidator : IRoleValidator<ApplicationRole>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IdentityErrorDescriber _describer;

        public TenantScopedRoleValidator(ApplicationDbContext dbContext, IdentityErrorDescriber describer)
        {
            _dbContext = dbContext;
            _describer = describer;
        }

        public async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(role.Name))
            {
                errors.Add(_describer.InvalidRoleName(role.Name));
                return IdentityResult.Failed(errors.ToArray());
            }

            var normalizedName = role.NormalizedName ?? manager.NormalizeKey(role.Name);
            if (await _dbContext.Roles.IgnoreQueryFilters().AnyAsync(r =>
                    r.Id != role.Id &&
                    r.NormalizedName == normalizedName &&
                    r.TenantId == role.TenantId))
            {
                errors.Add(_describer.DuplicateRoleName(role.Name));
            }

            return errors.Count > 0
                ? IdentityResult.Failed(errors.ToArray())
                : IdentityResult.Success;
        }
    }
}
