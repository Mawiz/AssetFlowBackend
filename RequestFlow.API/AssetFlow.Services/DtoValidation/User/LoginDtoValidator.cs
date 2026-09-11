using FluentValidation;
using AssetFlow.Common;
using AssetFlow.Services.Dto.User;
using System.Text.RegularExpressions;

namespace AssetFlow.Services.DtoValidation.User
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            //RuleFor(x => x.UserName)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.InValid, "username or password"));

            //RuleFor(x => x.Password)
            //    .NotEmpty()
            //    .WithMessage(string.Format(AppResource.InValid, "username or password"))
            //    .Length(8, 50)
            //    .WithMessage(string.Format(AppResource.InValid, "username or password"))
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
            //    .WithMessage(string.Format(AppResource.InValid, "username or password"));
        }
    }
}

