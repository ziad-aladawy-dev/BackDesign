using HUP.Application.DTOs.AcademicDtos.RoleDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminRolesController : ControllerBase
    {
        private readonly IRoleManagementService _roleService;
        private readonly ILogger<AdminRolesController> _logger;

        public AdminRolesController(
            IRoleManagementService roleService,
            ILogger<AdminRolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        #region ========== Roles ==========

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<RoleListDto>>> GetRoles(
            [FromQuery] RoleFilterDto? filter = null)
        {
            try
            {
                var result = await _roleService.GetRolesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, new { Message = "An error occurred while fetching roles" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDetailsDto>> GetRoleById(Guid id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFound(new { Message = $"Role with ID {id} not found" });

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting role {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching role" });
            }
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetRolesLookup()
        {
            try
            {
                var roles = await _roleService.GetRolesLookupAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles lookup");
                return StatusCode(500, new { Message = "An error occurred while fetching roles" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RoleListDto>>> SearchRoles([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { Message = "Search query is required" });

                var roles = await _roleService.SearchRolesAsync(q);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching roles with query: {q}");
                return StatusCode(500, new { Message = "An error occurred while searching roles" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RoleActionResponse>> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _roleService.CreateRoleAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetRoleById), new { id = result.Data }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { Message = "An error occurred while creating role" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoleActionResponse>> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _roleService.UpdateRoleAsync(id, dto);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role {id}");
                return StatusCode(500, new { Message = "An error occurred while updating role" });
            }
        }

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<RoleActionResponse>> ActivateRole(Guid id)
        {
            try
            {
                var result = await _roleService.ActivateRoleAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating role {id}");
                return StatusCode(500, new { Message = "An error occurred while activating role" });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<RoleActionResponse>> DeactivateRole(Guid id)
        {
            try
            {
                var result = await _roleService.DeactivateRoleAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating role {id}");
                return StatusCode(500, new { Message = "An error occurred while deactivating role" });
            }
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<ActionResult<RoleActionResponse>> SoftDeleteRole(Guid id)
        {
            try
            {
                var result = await _roleService.SoftDeleteRoleAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting role {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting role" });
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<RoleActionResponse>> RestoreRole(Guid id)
        {
            try
            {
                var result = await _roleService.RestoreRoleAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring role {id}");
                return StatusCode(500, new { Message = "An error occurred while restoring role" });
            }
        }

        [HttpDelete("{id}/hard")]
        public async Task<ActionResult<RoleActionResponse>> HardDeleteRole(Guid id)
        {
            try
            {
                var result = await _roleService.HardDeleteRoleAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error hard deleting role {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting role" });
            }
        }

        #endregion

        #region ========== Permissions ==========

        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
        {
            try
            {
                var permissions = await _roleService.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions");
                return StatusCode(500, new { Message = "An error occurred while fetching permissions" });
            }
        }

        [HttpGet("permissions/grouped")]
        public async Task<ActionResult<IEnumerable<PermissionsByCategoryDto>>> GetPermissionsGrouped()
        {
            try
            {
                var permissions = await _roleService.GetPermissionsGroupedByCategoryAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped permissions");
                return StatusCode(500, new { Message = "An error occurred while fetching permissions" });
            }
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(Guid id)
        {
            try
            {
                var permissions = await _roleService.GetRolePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting permissions for role {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching permissions" });
            }
        }

        [HttpPut("{id}/permissions")]
        public async Task<ActionResult<RoleActionResponse>> UpdateRolePermissions(Guid id, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var dto = new ManageRolePermissionsDto
                {
                    RoleId = id,
                    PermissionIds = permissionIds
                };

                var result = await _roleService.UpdateRolePermissionsAsync(dto);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for role {id}");
                return StatusCode(500, new { Message = "An error occurred while updating permissions" });
            }
        }

        [HttpPost("{id}/permissions/add")]
        public async Task<ActionResult<RoleActionResponse>> AddPermissionsToRole(Guid id, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var result = await _roleService.AddPermissionsToRoleAsync(id, permissionIds);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding permissions to role {id}");
                return StatusCode(500, new { Message = "An error occurred while adding permissions" });
            }
        }

        [HttpPost("{id}/permissions/remove")]
        public async Task<ActionResult<RoleActionResponse>> RemovePermissionsFromRole(Guid id, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var result = await _roleService.RemovePermissionsFromRoleAsync(id, permissionIds);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing permissions from role {id}");
                return StatusCode(500, new { Message = "An error occurred while removing permissions" });
            }
        }

        [HttpPost("copy-permissions")]
        public async Task<ActionResult<RoleActionResponse>> CopyPermissions([FromBody] CopyRolePermissionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _roleService.CopyPermissionsFromRoleAsync(dto);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying permissions");
                return StatusCode(500, new { Message = "An error occurred while copying permissions" });
            }
        }

        #endregion

        #region ========== User Assignment ==========

        [HttpPost("assign-user")]
        public async Task<ActionResult<RoleActionResponse>> AssignUserToRole([FromQuery] Guid userId, [FromQuery] Guid roleId)
        {
            try
            {
                var result = await _roleService.AssignUserToRoleAsync(userId, roleId);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning user {userId} to role {roleId}");
                return StatusCode(500, new { Message = "An error occurred while assigning user" });
            }
        }

        [HttpPost("remove-user/{userId}")]
        public async Task<ActionResult<RoleActionResponse>> RemoveUserFromRole(Guid userId)
        {
            try
            {
                var result = await _roleService.RemoveUserFromRoleAsync(userId);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user {userId} from role");
                return StatusCode(500, new { Message = "An error occurred while removing user" });
            }
        }

        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<UserInRoleDto>>> GetUsersInRole(Guid id)
        {
            try
            {
                var users = await _roleService.GetUsersInRoleAsync(id);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users in role {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        #endregion

        #region ========== Statistics & Reports ==========

        [HttpGet("statistics")]
        public async Task<ActionResult<RoleStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _roleService.GetRoleStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role statistics");
                return StatusCode(500, new { Message = "An error occurred while getting statistics" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] RoleFilterDto? filter = null)
        {
            try
            {
                var excelData = await _roleService.ExportRolesToExcelAsync(filter);
                var fileName = $"roles_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roles to Excel");
                return StatusCode(500, new { Message = "An error occurred while exporting roles" });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] RoleFilterDto? filter = null)
        {
            try
            {
                var csvData = await _roleService.ExportRolesToCsvAsync(filter);
                var fileName = $"roles_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roles to CSV");
                return StatusCode(500, new { Message = "An error occurred while exporting roles" });
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateReport([FromQuery] RoleFilterDto? filter = null)
        {
            try
            {
                var report = await _roleService.GenerateRolesReportAsync(filter);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating role report");
                return StatusCode(500, new { Message = "An error occurred while generating report" });
            }
        }

        #endregion
    }
}
