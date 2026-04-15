using ClosedXML.Excel;
using HUP.Application.DTOs.AcademicDtos.RoleDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Identity;
using HUP.Core.Entities.Permissions;
using HUP.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HUP.Application.Services.Implementations
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly HupDbContext _context;
        private readonly ILogger<RoleManagementService> _logger;

        public RoleManagementService(
            HupDbContext context,
            ILogger<RoleManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region ========== Roles Query Methods ==========

        public async Task<PaginatedResult<RoleListDto>> GetRolesAsync(RoleFilterDto? filter = null)
        {
            try
            {
                filter ??= new RoleFilterDto();
                var query = BuildRoleQuery(filter);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(r => new RoleListDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        DisplayName = r.DisplayName,
                        Description = r.Description,
                        UsersCount = r.Users.Count(u => !u.IsDeleted),
                        PermissionsCount = r.RolePermissions.Count,
                        IsActive = r.IsActive,
                        CreatedAt = r.CreatedAt,
                        CreatedByName = r.CreatedByUser != null ? r.CreatedByUser.FullName : null
                    })
                    .ToListAsync();

                return new PaginatedResult<RoleListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRolesAsync");
                throw;
            }
        }

        public async Task<RoleDetailsDto> GetRoleByIdAsync(Guid roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.Users.Where(u => !u.IsDeleted))
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                    return null;

                var allPermissions = await _context.Permissions
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .ToListAsync();

                var dto = new RoleDetailsDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    DisplayName = role.DisplayName,
                    Description = role.Description,
                    UsersCount = role.Users.Count,
                    PermissionsCount = role.RolePermissions.Count,
                    IsActive = role.IsActive,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt,
                    CreatedByName = role.CreatedByUser?.FullName,
                    Users = role.Users.Select(u => new UserInRoleDto
                    {
                        UserId = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        IsActive = u.IsActive
                    }).ToList(),
                    Permissions = allPermissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Category = GetPermissionCategory(p.Name),
                        IsAssigned = role.RolePermissions.Any(rp => rp.PermissionId == p.Id)
                    }).ToList()
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetRoleByIdAsync for ID {roleId}");
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetRolesLookupAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.DisplayName)
                .Select(r => new LookupDto
                {
                    Id = r.Id,
                    Name = r.DisplayName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<RoleListDto>> SearchRolesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<RoleListDto>();

            var roles = await _context.Roles
                .Where(r => !r.IsDeleted && (
                    r.Name.Contains(searchTerm) ||
                    r.DisplayName.Contains(searchTerm) ||
                    (r.Description != null && r.Description.Contains(searchTerm))))
                .Take(20)
                .Select(r => new RoleListDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    DisplayName = r.DisplayName,
                    Description = r.Description,
                    UsersCount = r.Users.Count(u => !u.IsDeleted),
                    PermissionsCount = r.RolePermissions.Count,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return roles;
        }

        #endregion

        #region ========== Roles Create/Update/Delete ==========

        public async Task<RoleActionResponse> CreateRoleAsync(CreateRoleDto dto)
        {
            try
            {
                var exists = await _context.Roles
                    .AnyAsync(r => (r.Name == dto.Name || r.DisplayName == dto.DisplayName) && !r.IsDeleted);

                if (exists)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = $"Role with name '{dto.Name}' or display name '{dto.DisplayName}' already exists"
                    };
                }

                var permissions = await _context.Permissions
                    .Where(p => dto.PermissionIds.Contains(p.Id) && !p.IsDeleted)
                    .ToListAsync();

                if (permissions.Count != dto.PermissionIds.Count)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "One or more permissions not found"
                    };
                }

                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    DisplayName = dto.DisplayName,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.Roles.AddAsync(role);

                foreach (var permissionId in dto.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };
                    await _context.RolePermissions.AddAsync(rolePermission);
                }

                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "Role created successfully",
                    Data = new { RoleId = role.Id }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error creating role: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> UpdateRoleAsync(Guid roleId, UpdateRoleDto dto)
        {
            try
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                if (!string.IsNullOrEmpty(dto.Name) && dto.Name != role.Name)
                {
                    var exists = await _context.Roles
                        .AnyAsync(r => r.Name == dto.Name && r.Id != roleId && !r.IsDeleted);

                    if (exists)
                    {
                        return new RoleActionResponse
                        {
                            Success = false,
                            Message = $"Role with name '{dto.Name}' already exists"
                        };
                    }

                    role.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.DisplayName) && dto.DisplayName != role.DisplayName)
                {
                    var exists = await _context.Roles
                        .AnyAsync(r => r.DisplayName == dto.DisplayName && r.Id != roleId && !r.IsDeleted);

                    if (exists)
                    {
                        return new RoleActionResponse
                        {
                            Success = false,
                            Message = $"Role with display name '{dto.DisplayName}' already exists"
                        };
                    }

                    role.DisplayName = dto.DisplayName;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                    role.Description = dto.Description;

                if (dto.IsActive.HasValue)
                    role.IsActive = dto.IsActive.Value;

                role.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "Role updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role {roleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error updating role: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> ActivateRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return new RoleActionResponse { Success = false, Message = "Role not found" };

            if (role.IsActive)
                return new RoleActionResponse { Success = false, Message = "Role is already active" };

            role.IsActive = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RoleActionResponse { Success = true, Message = "Role activated successfully" };
        }

        public async Task<RoleActionResponse> DeactivateRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return new RoleActionResponse { Success = false, Message = "Role not found" };

            if (!role.IsActive)
                return new RoleActionResponse { Success = false, Message = "Role is already inactive" };

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RoleActionResponse { Success = true, Message = "Role deactivated successfully" };
        }

        public async Task<RoleActionResponse> SoftDeleteRoleAsync(Guid roleId)
        {
            var hasUsers = await _context.Users
                .AnyAsync(u => u.RoleId == roleId && !u.IsDeleted);

            if (hasUsers)
            {
                return new RoleActionResponse
                {
                    Success = false,
                    Message = "Cannot delete role with assigned users. Reassign users first."
                };
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return new RoleActionResponse { Success = false, Message = "Role not found" };

            if (role.IsDeleted)
                return new RoleActionResponse { Success = false, Message = "Role is already deleted" };

            role.IsDeleted = true;
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RoleActionResponse { Success = true, Message = "Role soft deleted successfully" };
        }

        public async Task<RoleActionResponse> RestoreRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return new RoleActionResponse { Success = false, Message = "Role not found" };

            if (!role.IsDeleted)
                return new RoleActionResponse { Success = false, Message = "Role is not deleted" };

            role.IsDeleted = false;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RoleActionResponse { Success = true, Message = "Role restored successfully" };
        }

        public async Task<RoleActionResponse> HardDeleteRoleAsync(Guid roleId)
        {
            try
            {
                var hasUsers = await _context.Users.AnyAsync(u => u.RoleId == roleId);
                var hasPermissions = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId);

                if (hasUsers || hasPermissions)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Cannot delete role with existing users or permissions"
                    };
                }

                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                    return new RoleActionResponse { Success = false, Message = "Role not found" };

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "Role permanently deleted"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error hard deleting role {roleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error deleting role: {ex.Message}"
                };
            }
        }

        #endregion

        #region ========== Permissions Management ==========

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.DisplayName)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = GetPermissionCategory(p.Name)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PermissionsByCategoryDto>> GetPermissionsGroupedByCategoryAsync()
        {
            var permissions = await GetAllPermissionsAsync();

            return permissions
                .GroupBy(p => p.Category)
                .Select(g => new PermissionsByCategoryDto
                {
                    Category = g.Key,
                    Permissions = g.ToList()
                })
                .OrderBy(g => g.Category)
                .ToList();
        }

        public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

            if (role == null)
                return new List<PermissionDto>();

            return role.RolePermissions
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    DisplayName = rp.Permission.DisplayName,
                    Description = rp.Permission.Description,
                    Category = GetPermissionCategory(rp.Permission.Name),
                    IsAssigned = true
                })
                .ToList();
        }

        public async Task<RoleActionResponse> UpdateRolePermissionsAsync(ManageRolePermissionsDto dto)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoleId && !r.IsDeleted);

                if (role == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                var permissions = await _context.Permissions
                    .Where(p => dto.PermissionIds.Contains(p.Id) && !p.IsDeleted)
                    .ToListAsync();

                if (permissions.Count != dto.PermissionIds.Count)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "One or more permissions not found"
                    };
                }

                _context.RolePermissions.RemoveRange(role.RolePermissions);

                foreach (var permissionId in dto.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };
                    await _context.RolePermissions.AddAsync(rolePermission);
                }

                role.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "Role permissions updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for role {dto.RoleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error updating permissions: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> AddPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
                var newPermissionIds = permissionIds.Where(id => !existingPermissionIds.Contains(id)).ToList();

                if (!newPermissionIds.Any())
                {
                    return new RoleActionResponse
                    {
                        Success = true,
                        Message = "All permissions already assigned"
                    };
                }

                var permissions = await _context.Permissions
                    .Where(p => newPermissionIds.Contains(p.Id) && !p.IsDeleted)
                    .ToListAsync();

                if (permissions.Count != newPermissionIds.Count)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "One or more permissions not found"
                    };
                }

                foreach (var permissionId in newPermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };
                    await _context.RolePermissions.AddAsync(rolePermission);
                }

                role.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();


                return new RoleActionResponse
                {
                    Success = true,
                    Message = $"{newPermissionIds.Count} permissions added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding permissions to role {roleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error adding permissions: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                var permissionsToRemove = role.RolePermissions
                    .Where(rp => permissionIds.Contains(rp.PermissionId))
                    .ToList();

                if (!permissionsToRemove.Any())
                {
                    return new RoleActionResponse
                    {
                        Success = true,
                        Message = "No matching permissions found to remove"
                    };
                }

                _context.RolePermissions.RemoveRange(permissionsToRemove);
                role.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = $"{permissionsToRemove.Count} permissions removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing permissions from role {roleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error removing permissions: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> CopyPermissionsFromRoleAsync(CopyRolePermissionsDto dto)
        {
            try
            {
                var sourceRole = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.SourceRoleId && !r.IsDeleted);

                var targetRole = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.TargetRoleId && !r.IsDeleted);

                if (sourceRole == null || targetRole == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "One or both roles not found"
                    };
                }

                var sourcePermissionIds = sourceRole.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
                var targetPermissionIds = targetRole.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

                if (dto.OverwriteExisting)
                {
                    _context.RolePermissions.RemoveRange(targetRole.RolePermissions);

                    foreach (var permId in sourcePermissionIds)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = targetRole.Id,
                            PermissionId = permId
                        };
                        await _context.RolePermissions.AddAsync(rolePermission);
                    }
                }
                else
                {
                    var newPermissionIds = sourcePermissionIds
                        .Where(id => !targetPermissionIds.Contains(id))
                        .ToList();

                    foreach (var permId in newPermissionIds)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = targetRole.Id,
                            PermissionId = permId
                        };
                        await _context.RolePermissions.AddAsync(rolePermission);
                    }
                }

                targetRole.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "Permissions copied successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying permissions between roles");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error copying permissions: {ex.Message}"
                };
            }
        }

        public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName)
        {
            return await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId &&
                               rp.Permission.Name == permissionName &&
                               !rp.Role.IsDeleted &&
                               rp.Role.IsActive);
        }

        #endregion

        #region ========== User Role Assignment ==========

        public async Task<RoleActionResponse> AssignUserToRoleAsync(Guid userId, Guid roleId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

                if (role == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                user.RoleId = roleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = $"User assigned to role '{role.DisplayName}' successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning user {userId} to role {roleId}");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error assigning user: {ex.Message}"
                };
            }
        }

        public async Task<RoleActionResponse> RemoveUserFromRoleAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "User" && !r.IsDeleted);

                if (defaultRole == null)
                {
                    return new RoleActionResponse
                    {
                        Success = false,
                        Message = "Cannot remove user role: No default role found"
                    };
                }

                var oldRoleId = user.RoleId;
                user.RoleId = defaultRole.Id;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new RoleActionResponse
                {
                    Success = true,
                    Message = "User removed from role successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user {userId} from role");
                return new RoleActionResponse
                {
                    Success = false,
                    Message = $"Error removing user: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(Guid roleId)
        {
            return await _context.Users
                .Where(u => u.RoleId == roleId && !u.IsDeleted && u.IsActive)
                .Select(u => new UserInRoleDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }

        #endregion

        #region ========== Statistics ==========

        public async Task<RoleStatisticsDto> GetRoleStatisticsAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            var stats = new RoleStatisticsDto
            {
                TotalRoles = roles.Count,
                ActiveRoles = roles.Count(r => r.IsActive),
                InactiveRoles = roles.Count(r => !r.IsActive),
                TotalPermissions = await _context.Permissions.CountAsync(p => !p.IsDeleted),
                TotalUsersAssigned = roles.Sum(r => r.Users.Count(u => !u.IsDeleted)),
                GeneratedAt = DateTime.UtcNow
            };

            stats.RolesByUsersCount = roles
                .Where(r => r.Users.Count > 0)
                .OrderByDescending(r => r.Users.Count)
                .Take(5)
                .ToDictionary(
                    r => r.DisplayName,
                    r => r.Users.Count
                );

            var permissionUsage = await _context.RolePermissions
                .GroupBy(rp => rp.PermissionId)
                .Select(g => new
                {
                    PermissionId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var permissionNames = await _context.Permissions
                .Where(p => permissionUsage.Select(x => x.PermissionId).Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.DisplayName);

            stats.PermissionsDistribution = permissionUsage
                .Select(x => new RolePermissionSummaryDto
                {
                    PermissionName = permissionNames.GetValueOrDefault(x.PermissionId, "Unknown"),
                    RolesCount = x.Count
                })
                .ToList();

            return stats;
        }

        #endregion

        #region ========== Export ==========

        public async Task<byte[]> ExportRolesToExcelAsync(RoleFilterDto? filter = null)
        {
            var roles = await GetRolesAsync(filter ?? new RoleFilterDto { PageSize = 1000 });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Roles");

                var headers = new[] {
                    "ID", "Name", "Display Name", "Description",
                    "Users Count", "Permissions Count", "Status", "Created Date", "Created By"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var role in roles.Items)
                {
                    worksheet.Cell(row, 1).Value = role.Id.ToString();
                    worksheet.Cell(row, 2).Value = role.Name;
                    worksheet.Cell(row, 3).Value = role.DisplayName;
                    worksheet.Cell(row, 4).Value = role.Description;
                    worksheet.Cell(row, 5).Value = role.UsersCount;
                    worksheet.Cell(row, 6).Value = role.PermissionsCount;
                    worksheet.Cell(row, 7).Value = role.IsActive ? "Active" : "Inactive";
                    worksheet.Cell(row, 8).Value = role.CreatedAt.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 9).Value = role.CreatedByName;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportRolesToCsvAsync(RoleFilterDto? filter = null)
        {
            var roles = await GetRolesAsync(filter ?? new RoleFilterDto { PageSize = 1000 });

            var csv = new StringBuilder();
            csv.AppendLine("ID,Name,Display Name,Description,Users Count,Permissions Count,Status,Created Date,Created By");

            foreach (var role in roles.Items)
            {
                csv.AppendLine($"\"{role.Id}\",\"{role.Name}\",\"{role.DisplayName}\",\"{role.Description}\",{role.UsersCount},{role.PermissionsCount},\"{(role.IsActive ? "Active" : "Inactive")}\",\"{role.CreatedAt:yyyy-MM-dd}\",\"{role.CreatedByName}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<string> GenerateRolesReportAsync(RoleFilterDto? filter = null)
        {
            var roles = await GetRolesAsync(filter ?? new RoleFilterDto());
            var stats = await GetRoleStatisticsAsync();

            var report = new StringBuilder();

            report.AppendLine("=== Roles & Permissions Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("=== Summary ===");
            report.AppendLine($"Total Roles: {stats.TotalRoles}");
            report.AppendLine($"Active Roles: {stats.ActiveRoles}");
            report.AppendLine($"Inactive Roles: {stats.InactiveRoles}");
            report.AppendLine($"Total Permissions: {stats.TotalPermissions}");
            report.AppendLine($"Total Users Assigned: {stats.TotalUsersAssigned}");
            report.AppendLine();
            report.AppendLine("=== Roles with Most Users ===");
            foreach (var kvp in stats.RolesByUsersCount)
                report.AppendLine($"{kvp.Key}: {kvp.Value} users");
            report.AppendLine();
            report.AppendLine("=== Most Used Permissions ===");
            foreach (var perm in stats.PermissionsDistribution)
                report.AppendLine($"{perm.PermissionName}: in {perm.RolesCount} roles");
            report.AppendLine();
            report.AppendLine("=== All Roles ===");
            foreach (var role in roles.Items.Take(10))
            {
                report.AppendLine($"{role.DisplayName} ({role.Name}) - {role.UsersCount} users, {role.PermissionsCount} permissions - {(role.IsActive ? "Active" : "Inactive")}");
            }

            return report.ToString();
        }

        #endregion

        #region ========== Private Helper Methods ==========

        private IQueryable<Role> BuildRoleQuery(RoleFilterDto filter)
        {
            var query = _context.Roles
                .Include(r => r.Users)
                .Include(r => r.CreatedByUser)
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.DisplayName.ToLower().Contains(searchTerm) ||
                    (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(r => r.IsActive == filter.IsActive.Value);

            if (filter.MinUsers.HasValue)
                query = query.Where(r => r.Users.Count(u => !u.IsDeleted) >= filter.MinUsers.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(r => r.Name)
                    : query.OrderBy(r => r.Name),
                "displayname" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(r => r.DisplayName)
                    : query.OrderBy(r => r.DisplayName),
                "users" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(r => r.Users.Count)
                    : query.OrderBy(r => r.Users.Count),
                "permissions" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(r => r.RolePermissions.Count)
                    : query.OrderBy(r => r.RolePermissions.Count),
                _ => filter.SortOrder == "desc"
                    ? query.OrderByDescending(r => r.DisplayName)
                    : query.OrderBy(r => r.DisplayName)
            };

            return query;
        }

        private string GetPermissionCategory(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
                return "Other";

            if (permissionName.StartsWith("Create")) return "Create";
            if (permissionName.StartsWith("Add")) return "Create";
            if (permissionName.StartsWith("New")) return "Create";

            if (permissionName.StartsWith("Edit")) return "Edit";
            if (permissionName.StartsWith("Update")) return "Edit";
            if (permissionName.StartsWith("Modify")) return "Edit";

            if (permissionName.StartsWith("Delete")) return "Delete";
            if (permissionName.StartsWith("Remove")) return "Delete";

            if (permissionName.StartsWith("View")) return "View";
            if (permissionName.StartsWith("Read")) return "View";
            if (permissionName.StartsWith("Get")) return "View";

            if (permissionName.StartsWith("Manage")) return "Management";
            if (permissionName.StartsWith("Admin")) return "Administration";

            if (permissionName.Contains("User")) return "Users";
            if (permissionName.Contains("Role")) return "Roles";
            if (permissionName.Contains("Permission")) return "Permissions";

            if (permissionName.Contains("Course")) return "Courses";
            if (permissionName.Contains("Faculty")) return "Faculties";
            if (permissionName.Contains("Department")) return "Departments";

            if (permissionName.Contains("Report")) return "Reports";
            if (permissionName.Contains("Export")) return "Reports";
            if (permissionName.Contains("Import")) return "Data Management";

            if (permissionName.Contains("Student")) return "Students";
            if (permissionName.Contains("Instructor")) return "Instructors";
            if (permissionName.Contains("Teacher")) return "Instructors";

            if (permissionName.Contains("Enrollment")) return "Enrollments";
            if (permissionName.Contains("Grade")) return "Grades";
            if (permissionName.Contains("Exam")) return "Exams";
            if (permissionName.Contains("Schedule")) return "Schedules";

            if (permissionName.Contains("Setting")) return "Settings";
            if (permissionName.Contains("Config")) return "Settings";

            return "Other";
        }

        #endregion
    }
}
