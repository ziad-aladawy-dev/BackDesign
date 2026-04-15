using HUP.Application.DTOs.AcademicDtos.Enrollment;

namespace HUP.Application.Validators.Interfaces
{
    public interface IEnrollmentValidator
    {
        Task ValidateEnrollmentAsync(List<CreateEnrollmentDto> dtos);
        Task ValidateDropAsync(Guid enrollmentId, Guid studentId);
    }
}
