using FluentValidation;
using HUP.Application.DTOs.IdentityDtos.UserDtos;

namespace HUP.Application.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.NationalId).NotEmpty().Length(14);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.PasswordHash).NotEmpty().MinimumLength(6);
            RuleFor(x => x.FullName).NotNull();
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}
