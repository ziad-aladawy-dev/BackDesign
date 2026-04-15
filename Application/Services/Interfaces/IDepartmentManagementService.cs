using HUP.Application.DTOs.AcademicDtos.DepartmentDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IDepartmentManagementService
    {
        #region Query Methods
        Task<PaginatedResult<DepartmentListDto>> GetDepartmentsAsync(DepartmentFilterDto? filter = null);
        Task<DepartmentDetailsDto> GetDepartmentByIdAsync(Guid departmentId);
        Task<IEnumerable<LookupDto>> GetDepartmentsLookupAsync(Guid? facultyId = null);
        Task<IEnumerable<LookupDto>> GetDepartmentsByFacultyLookupAsync(Guid facultyId);
        Task<IEnumerable<string>> GetDepartmentCodesAsync();
        Task<IEnumerable<DepartmentCourseDto>> GetDepartmentCoursesAsync(Guid departmentId);
        Task<IEnumerable<DepartmentListDto>> SearchDepartmentsAsync(string searchTerm);
        #endregion

        #region Create Methods
        Task<DepartmentActionResponse> CreateDepartmentAsync(CreateDepartmentDto dto);
        #endregion

        #region Update Methods
        Task<DepartmentActionResponse> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentDto dto);
        #endregion

        #region Head of Department Management
        Task<DepartmentActionResponse> AssignHeadOfDepartmentAsync(Guid departmentId, Guid instructorId);
        Task<DepartmentActionResponse> RemoveHeadOfDepartmentAsync(Guid departmentId);
        #endregion

        #region Course Management
        Task<DepartmentActionResponse> AddCourseToProgramAsync(Guid departmentId, ManageDepartmentCourseDto dto);
        Task<DepartmentActionResponse> RemoveCourseFromProgramAsync(Guid departmentId, Guid courseId);
        #endregion

        #region Transfer Operations
        Task<DepartmentActionResponse> TransferItemsAsync(TransferDepartmentItemsDto dto);
        #endregion

        #region Status Change Methods
        Task<DepartmentActionResponse> ActivateDepartmentAsync(Guid departmentId);
        Task<DepartmentActionResponse> DeactivateDepartmentAsync(Guid departmentId);
        Task<DepartmentActionResponse> SoftDeleteDepartmentAsync(Guid departmentId);
        Task<DepartmentActionResponse> RestoreDepartmentAsync(Guid departmentId);
        #endregion

        #region Statistics & Reports
        Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync();
        Task<byte[]> ExportDepartmentsToExcelAsync(DepartmentFilterDto? filter = null);
        Task<byte[]> ExportDepartmentsToCsvAsync(DepartmentFilterDto? filter = null);
        Task<string> GenerateDepartmentsReportAsync(DepartmentFilterDto? filter = null);
        #endregion
    }
}