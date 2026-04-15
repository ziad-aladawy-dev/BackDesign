using HUP.Application.DTOs.AcademicDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<IEnumerable<FacultyDto>> GetAllAsync(string lang);
        Task<FacultyDto?> GetByIdAsync(Guid id, string lang);
    }
}