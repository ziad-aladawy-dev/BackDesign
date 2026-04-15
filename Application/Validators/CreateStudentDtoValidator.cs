using FluentValidation;
using HUP.Application.DTOs.AcademicDtos.Student;
using HUP.Application.DTOs.IdentityDtos.UserDtos;

namespace HUP.Application.Validators
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.UniversityCode).NotEmpty();
            RuleFor(x => x.UniversityEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.DepartmentId).NotEmpty();
            RuleFor(x => x.Level).GreaterThan(0);
            RuleFor(x => x.UserInfo).NotNull().SetValidator(new CreateUserDtoValidator());
        }
    }
}
