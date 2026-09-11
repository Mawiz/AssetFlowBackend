using FluentValidation;
using AssetFlow.Common;
using AssetFlow.Services.Dto.User;
using System.Text.RegularExpressions;

namespace AssetFlow.Services.DtoValidation.User
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Id"));

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Password"))
                .Length(8, 50)
                .WithMessage($"Password {string.Format(AppResource.CanNotBeLessThan, "8")} and {string.Format(AppResource.CanNotBeMoreThan, "50")}")
                .Must(x =>
                {
                    if (x == null)
                        return false;
                    //Password must be of minimum 8 characters & should contain:
                    //Atleast 1 number,a special character,a upper and a lower case letter.

                    if (Regex.IsMatch(x, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[!\"#$%&'()*+,-.\\/\\:;<=>?@[\\]^_`{|}~]).{8,}$"))
                        return true;
                    return false;
                })
                .WithMessage(string.Format(AppResource.PasswordNotStrongEnough));

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Token")).
                Must(x =>
                {
                    if (!string.IsNullOrEmpty(x) && string.IsNullOrWhiteSpace(x))
                        return false;
                    return true;
                })
                .WithMessage(string.Format(AppResource.InValid, "Token"));
        }
    }
}
