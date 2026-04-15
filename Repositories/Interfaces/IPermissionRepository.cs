using HUP.Core.Entities.Permissions;

namespace HUP.Repositories.Interfaces;

public interface IPermissionRepository : IGenericRepository<Permission>
{
    Task<List<string>> GetAllPermissionsForRole(Guid roleId);
    void UpdatePermission(Permission permission);
    Task AddRolePermission(Guid permissionId, Guid roleId);
    void DeleteRolePermission(Guid permissionId, Guid roleId);
}