using FluentValidation;
using RequestFlow.Common;
using RequestFlow.Services.Dto.User;

namespace RequestFlow.Services.DtoValidation.User
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
