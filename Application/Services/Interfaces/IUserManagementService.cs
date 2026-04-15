using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.AcademicDtos.UserDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto filter);
        Task<UserDetailsDto> GetUserByIdAsync(Guid userId);
        Task<UserActionResponse> CreateUserAsync(AdminCreateUserDto dto);
        Task<UserActionResponse> UpdateUserAsync(Guid userId, UpdateUserDto dto);

        Task<UserActionResponse> ActivateUserAsync(Guid userId);
        Task<UserActionResponse> DeactivateUserAsync(Guid userId, string reason = null);
        Task<UserActionResponse> SoftDeleteUserAsync(Guid userId, string reason = null);
        Task<UserActionResponse> RestoreUserAsync(Guid userId);
        Task<UserActionResponse> HardDeleteUserAsync(Guid userId);

        Task<BulkOperationResult> BulkUpdateUsersAsync(BulkUserActionDto dto);

        Task<UserActionResponse> ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
        Task<UserActionResponse> ForcePasswordChangeAsync(Guid userId);
        Task<UserActionResponse> ExtendPasswordExpiryAsync(Guid userId, int months);

        Task<IEnumerable<UserListDto>> SearchUsersAsync(string searchTerm);

        Task<UserStatisticsDto> GetUserStatisticsAsync();

        Task<byte[]> ExportUsersToExcelAsync(UserFilterDto filter = null);
        Task<byte[]> ExportUsersToCsvAsync(UserFilterDto filter = null);
        Task<string> GenerateUsersReportAsync(UserFilterDto filter = null);

        Task<IEnumerable<UserListDto>> GetUsersByRoleAsync(Guid roleId);
        Task<IEnumerable<UserListDto>> GetUsersByFacultyAsync(Guid facultyId);
        Task<IEnumerable<UserListDto>> GetUsersByDepartmentAsync(Guid departmentId);
        Task<IEnumerable<UserListDto>> GetUsersByTypeAsync(string userType);
        Task<IEnumerable<UserListDto>> GetInactiveUsersAsync();
        Task<IEnumerable<UserListDto>> GetPasswordExpiredUsersAsync();
    }
}
