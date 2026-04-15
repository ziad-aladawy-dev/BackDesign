using HUP.Application.DTOs.AcademicDtos.CourseDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface ICourseManagementService
    {
        Task<PaginatedResult<CourseListDto>> GetCoursesAsync(CourseFilterDto? filter = null);
        Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId);
        Task<CourseActionResponse> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseActionResponse> UpdateCourseAsync(Guid courseId, UpdateCourseDto dto);

        Task<CourseActionResponse> ActivateCourseAsync(Guid courseId);
        Task<CourseActionResponse> DeactivateCourseAsync(Guid courseId);
        Task<CourseActionResponse> SoftDeleteCourseAsync(Guid courseId);
        Task<CourseActionResponse> RestoreCourseAsync(Guid courseId);

        Task<CourseActionResponse> AddPrerequisiteAsync(Guid courseId, Guid prerequisiteId);
        Task<CourseActionResponse> RemovePrerequisiteAsync(Guid courseId);
        Task<IEnumerable<CoursePrerequisiteDto>> GetPrerequisiteChainAsync(Guid courseId);

        Task<CourseActionResponse> AddDepartmentsAsync(Guid courseId, ManageCourseDepartmentsDto dto);
        Task<CourseActionResponse> RemoveDepartmentAsync(Guid courseId, Guid departmentId);
        Task<IEnumerable<CourseDepartmentDto>> GetCourseDepartmentsAsync(Guid courseId);

        Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string searchTerm);

        Task<CourseStatisticsDto> GetCourseStatisticsAsync();

        Task<IEnumerable<LookupDto>> GetCoursesByDepartmentLookupAsync(Guid departmentId);

        Task<byte[]> ExportCoursesToExcelAsync(CourseFilterDto? filter = null);
        Task<byte[]> ExportCoursesToCsvAsync(CourseFilterDto? filter = null);
        Task<string> GenerateCoursesReportAsync(CourseFilterDto? filter = null);
    }
}
