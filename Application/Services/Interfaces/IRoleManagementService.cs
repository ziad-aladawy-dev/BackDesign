using HUP.Application.DTOs.AcademicDtos.RoleDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IRoleManagementService
    {
        Task<PaginatedResult<RoleListDto>> GetRolesAsync(RoleFilterDto? filter = null);
        Task<RoleDetailsDto> GetRoleByIdAsync(Guid roleId);

        Task<RoleActionResponse> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleActionResponse> UpdateRoleAsync(Guid roleId, UpdateRoleDto dto);

        Task<RoleActionResponse> ActivateRoleAsync(Guid roleId);
        Task<RoleActionResponse> DeactivateRoleAsync(Guid roleId);
        Task<RoleActionResponse> SoftDeleteRoleAsync(Guid roleId);
        Task<RoleActionResponse> RestoreRoleAsync(Guid roleId);

        Task<RoleActionResponse> HardDeleteRoleAsync(Guid roleId);

        Task<IEnumerable<RoleListDto>> SearchRolesAsync(string searchTerm);

        Task<IEnumerable<LookupDto>> GetRolesLookupAsync();


        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<IEnumerable<PermissionsByCategoryDto>> GetPermissionsGroupedByCategoryAsync();
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId);

        Task<RoleActionResponse> UpdateRolePermissionsAsync(ManageRolePermissionsDto dto);
        Task<RoleActionResponse> AddPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds);
        Task<RoleActionResponse> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds);

        Task<RoleActionResponse> CopyPermissionsFromRoleAsync(CopyRolePermissionsDto dto);

        Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName);


        Task<RoleActionResponse> AssignUserToRoleAsync(Guid userId, Guid roleId);
        Task<RoleActionResponse> RemoveUserFromRoleAsync(Guid userId);
        Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(Guid roleId);


        Task<RoleStatisticsDto> GetRoleStatisticsAsync();

        Task<byte[]> ExportRolesToExcelAsync(RoleFilterDto? filter = null);
        Task<byte[]> ExportRolesToCsvAsync(RoleFilterDto? filter = null);
        Task<string> GenerateRolesReportAsync(RoleFilterDto? filter = null);
    }
}
