using System.ComponentModel.DataAnnotations;
using AssetFlow.Data.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Identity
{
    /// <summary>
    /// Replaces the default Identity user validator so username/email are unique per tenant, not globally.
    /// </summary>
    public class TenantScopedUserValidator : IUserValidator<ApplicationUser>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IdentityErrorDescriber _describer;

        public TenantScopedUserValidator(ApplicationDbContext dbContext, IdentityErrorDescriber describer)
        {
            _dbContext = dbContext;
            _describer = describer;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(_describer.InvalidUserName(user.UserName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                     user.UserName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors.Add(_describer.InvalidUserName(user.UserName));
            }

            if (!string.IsNullOrWhiteSpace(user.Email) &&
                !new EmailAddressAttribute().IsValid(user.Email))
            {
                errors.Add(_describer.InvalidEmail(user.Email));
            }

            var normalizedUserName = user.NormalizedUserName ?? manager.NormalizeName(user.UserName);
            if (!string.IsNullOrEmpty(normalizedUserName) &&
                await _dbContext.ApplicationUsers.IgnoreQueryFilters().AnyAsync(u =>
                    u.Id != user.Id &&
                    u.NormalizedUserName == normalizedUserName &&
                    u.TenantId == user.TenantId))
            {
                errors.Add(_describer.DuplicateUserName(user.UserName));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var normalizedEmail = user.NormalizedEmail ?? manager.NormalizeEmail(user.Email);
                if (await _dbContext.ApplicationUsers.IgnoreQueryFilters().AnyAsync(u =>
                        u.Id != user.Id &&
                        u.NormalizedEmail == normalizedEmail &&
                        u.TenantId == user.TenantId))
                {
                    errors.Add(_describer.DuplicateEmail(user.Email));
                }
            }

            return errors.Count > 0
                ? IdentityResult.Failed(errors.ToArray())
                : IdentityResult.Success;
        }
    }
}
