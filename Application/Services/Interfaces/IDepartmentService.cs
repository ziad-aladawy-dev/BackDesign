using HUP.Application.DTOs.AcademicDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllAsync(string lang);
        Task<DepartmentDto?> GetByIdAsync(Guid id, string lang);
    }
}