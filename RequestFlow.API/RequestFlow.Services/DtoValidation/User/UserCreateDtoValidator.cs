using FluentValidation;
using RequestFlow.Common;
using RequestFlow.Services.Dto.User;
using System.Text.RegularExpressions;

namespace RequestFlow.Services.DtoValidation.User
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            //RuleFor(x => x.UserName)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.CanNotBeEmpty, "User Name"));

            //RuleFor(x => x.FirstLetter)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.CanNotBeEmpty, "First Letter"));

            //RuleFor(x => x.FullName)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Full Name"))
            //    .MaximumLength(50)
            //    .WithMessage(string.Format(AppResource.CanNotBeMoreThan, "50 Letters"));

            //RuleFor(x => x.Email)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Email Address"))
            //    .EmailAddress()
            //    .WithMessage(string.Format(AppResource.NotAValidEmailAddress));

            //RuleFor(x => x.Password)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Password"))
            //    .Length(8, 50)
            //    .WithMessage($"Password {string.Format(AppResource.CanNotBeLessThan, "8")} and {string.Format(AppResource.CanNotBeMoreThan, "50")}")
            //    .Must(x =>
            //    {
            //        if (x == null)
            //            return false;
            //        //Password must be of minimum 8 characters & should contain:
            //        //Atleast 1 number,a special character,a upper and a lower case letter.

            //        if (Regex.IsMatch(x, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[!\"#$%&'()*+,-.\\/\\:;<=>?@[\\]^_`{|}~]).{8,}$"))
            //            return true;
            //        return false;
            //    })
            //    .WithMessage(string.Format(AppResource.PasswordNotStrongeEnough));

            //RuleFor(x => x.RoleId)
            //    .IsInEnum()
            //    .WithMessage(string.Format(AppResource.InValid, "Role"));
        }
    }
}
