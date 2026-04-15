using HUP.Application.DTOs.AcademicDtos.Enrollment;
using HUP.Application.DTOs.AcademicDtos;

namespace HUP.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task AddAsync(List<CreateEnrollmentDto> dtos);
    Task<IEnumerable<EnrollmentResponseDto>> GetAllAsync(string lang);
    Task<EnrollmentResponseDto> GetByIdAsync(Guid id, string lang);
    Task<bool> Exists(CreateEnrollmentDto dto);
    Task Remove(Guid id);
    Task SoftDelete(Guid id);
    Task Update(Guid id, UpdateEnrollmentDto dto);
    Task<List<SemesterTranscriptDto>> GetStudentGradesAsync(Guid studentId, string lang);
    Task<IEnumerable<EnrollmentResponseDto>> GetRegisteredByStudentAsync(Guid studentId, string lang, EnrollmentFilterDto filter);
    Task<bool> CanStudentEnroll(Guid studentId);
}