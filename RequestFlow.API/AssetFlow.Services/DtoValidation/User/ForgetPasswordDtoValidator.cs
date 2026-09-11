using FluentValidation;
using AssetFlow.Common;
using AssetFlow.Services.Dto.User;

namespace AssetFlow.Services.DtoValidation.User
{
    public class ForgetPasswordDtoValidator : AbstractValidator<ForgetPasswordDto>
    {
        public ForgetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(string.Format(AppResource.CanNotBeEmpty, "Email Address"))
                .EmailAddress()
                .WithMessage(string.Format(AppResource.NotAValidEmailAddress));
        }
    }
}
