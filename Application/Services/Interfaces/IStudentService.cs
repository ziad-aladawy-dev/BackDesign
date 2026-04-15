using HUP.Application.DTOs.AcademicDtos.Student;
using HUP.Application.DTOs.AcademicDtos;
using HUP.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace HUP.Application.Services.Interfaces;

public interface IStudentService
{
    Task<StudentProfileDto> GetStudentProfile(Guid userId, string lang);
    Task AddStudent(CreateStudentDto dto);
    Task<bool> UpdateStudentStatus(StudentStatusDto dto);
    Task<string> UploadProfilePhotoAsync(Guid studentId, IFormFile file);
}
