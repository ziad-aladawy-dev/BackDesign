using HUP.Application.DTOs.AcademicDtos.FacultyDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IFacultyManagementService
    {
        Task<PaginatedResult<FacultyListDto>> GetFacultiesAsync(FacultyFilterDto? filter = null);
        Task<FacultyDetailsDto?> GetFacultyByIdAsync(Guid facultyId);
        Task<IEnumerable<LookupDto>> GetFacultiesLookupAsync();

        Task<IEnumerable<FacultyListDto>> SearchFacultiesAsync(string searchTerm);

        Task<FacultyActionResponse> CreateFacultyAsync(CreateFacultyDto dto);
        Task<FacultyActionResponse> UpdateFacultyAsync(Guid facultyId, UpdateFacultyDto dto);

        Task<FacultyActionResponse> AssignDeanAsync(Guid facultyId, Guid instructorId);
        Task<FacultyActionResponse> RemoveDeanAsync(Guid facultyId);

        Task<FacultyActionResponse> ActivateFacultyAsync(Guid facultyId);
        Task<FacultyActionResponse> DeactivateFacultyAsync(Guid facultyId);
        Task<FacultyActionResponse> SoftDeleteFacultyAsync(Guid facultyId);
        Task<FacultyActionResponse> RestoreFacultyAsync(Guid facultyId);

        Task<FacultyStatisticsDto> GetFacultyStatisticsAsync();

        Task<byte[]> ExportFacultiesToExcelAsync(FacultyFilterDto? filter = null);
        Task<byte[]> ExportFacultiesToCsvAsync(FacultyFilterDto? filter = null);
    }
}