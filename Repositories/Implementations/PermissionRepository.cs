using HUP.Core.Entities.Permissions;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(HupDbContext context) : base(context)
    {
    }

    public async Task<List<string>> GetAllPermissionsForRole(Guid roleId)
    {
        var permissionNames = await _context.RolePermissions.Where(r => r.RoleId == roleId)
            .Select(r => r.Permission.Name).AsNoTracking().ToListAsync();
        return permissionNames;
    }

    public void UpdatePermission(Permission permission)
    {
        _context.Permissions.Update(permission);
    }

    public async Task AddRolePermission(Guid permissionId, Guid roleId)
    {
        RolePermission relation = new RolePermission();
        relation.RoleId = roleId;
        relation.PermissionId = permissionId;
        await _context.RolePermissions.AddAsync(relation);
    }

    public void DeleteRolePermission(Guid permissionId, Guid roleId)
    {
        var relation = _context.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId
                                                                     && rp.RoleId == roleId);
        if (relation != null)
            _context.RolePermissions.Remove(relation);
    }
}