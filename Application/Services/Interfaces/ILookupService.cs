using HUP.Application.DTOs.LookupDtos;
using HUP.Core.Enums;

namespace HUP.Application.Services.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<FacultyLookupDto>> GetAllFacultiesAsync();
        Task<IEnumerable<DepartmentLookupDto>> GetDepartmentsByFacultyAsync(Guid facultyId);
        Task<IEnumerable<DepartmentLookupDto>> GetAllDepartmentsAsync();
        Task<IEnumerable<RoleLookupDto>> GetAllRolesAsync();
        Task<IEnumerable<string>> GetAllUserTypesAsync();
        Task<IEnumerable<AcademicStatus>> GetAllAcademicStatusesAsync();
    }
}
