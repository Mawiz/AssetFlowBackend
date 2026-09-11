using FluentValidation;
using AssetFlow.Common;
using AssetFlow.Services.Dto.User;
using System.Text.RegularExpressions;

namespace AssetFlow.Services.DtoValidation.User
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Id"));

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Full Name"))
                .MaximumLength(50)
                .WithMessage(string.Format(AppResource.CanNotBeMoreThan, "50 Letters"));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Email Address"))
                .EmailAddress()
                .WithMessage(string.Format(AppResource.NotAValidEmailAddress));

            RuleFor(x => x.Password)
               .Must(x =>
                {
                    if (string.IsNullOrEmpty(x))
                        return true;
                    //Password must be of minimum 8 characters & should contain:
                    //Atleast 1 number,a special character,a upper and a lower case letter.

                    if (Regex.IsMatch(x, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[!\"#$%&'()*+,-.\\/\\:;<=>?@[\\]^_`{|}~]).{8,}$"))
                        return true;
                    return false;
                })
                .WithMessage(string.Format(AppResource.PasswordNotStrongEnough));

            RuleFor(x => x.RoleId)
                .IsInEnum()
                .WithMessage(string.Format(AppResource.InValid, "Role"));
        }
    }
}
