using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.AcademicDtos.UserDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserManagementService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region Query Endpoints

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<UserListDto>>> GetUsers([FromQuery] UserFilterDto? filter = null)
        {
            try
            {
                var result = await _userService.GetUsersAsync(filter ?? new UserFilterDto());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailsDto>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Message = $"User with ID {id} not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching user" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> SearchUsers([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { Message = "Search query is required" });

                var users = await _userService.SearchUsersAsync(q);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching users with query: {q}");
                return StatusCode(500, new { Message = "An error occurred while searching users" });
            }
        }

        #endregion

        #region Create Endpoints

        [HttpPost]
        public async Task<ActionResult<UserActionResponse>> CreateUser([FromBody] AdminCreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.CreateUserAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { Message = "An error occurred while creating user" });
            }
        }

        #endregion

        #region Update Endpoints

        [HttpPut("{id}")]
        public async Task<ActionResult<UserActionResponse>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.UpdateUserAsync(id, dto);

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
                _logger.LogError(ex, $"Error updating user {id}");
                return StatusCode(500, new { Message = "An error occurred while updating user" });
            }
        }

        #endregion

        #region Status Change Endpoints

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<UserActionResponse>> ActivateUser(Guid id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);

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
                _logger.LogError(ex, $"Error activating user {id}");
                return StatusCode(500, new { Message = "An error occurred while activating user" });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<UserActionResponse>> DeactivateUser(Guid id, [FromBody] string? reason = null)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id, reason);

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
                _logger.LogError(ex, $"Error deactivating user {id}");
                return StatusCode(500, new { Message = "An error occurred while deactivating user" });
            }
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<ActionResult<UserActionResponse>> SoftDeleteUser(Guid id, [FromBody] string? reason = null)
        {
            try
            {
                var result = await _userService.SoftDeleteUserAsync(id, reason);

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
                _logger.LogError(ex, $"Error soft deleting user {id}");
                return StatusCode(500, new { Message = "An error occurred while soft deleting user" });
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<UserActionResponse>> RestoreUser(Guid id)
        {
            try
            {
                var result = await _userService.RestoreUserAsync(id);

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
                _logger.LogError(ex, $"Error restoring user {id}");
                return StatusCode(500, new { Message = "An error occurred while restoring user" });
            }
        }

        [HttpDelete("{id}/hard")]
        public async Task<ActionResult<UserActionResponse>> HardDeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.HardDeleteUserAsync(id);

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
                _logger.LogError(ex, $"Error hard deleting user {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting user" });
            }
        }

        #endregion

        #region Bulk Operations

        [HttpPost("bulk")]
        public async Task<ActionResult<BulkOperationResult>> BulkUpdateUsers([FromBody] BulkUserActionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.BulkUpdateUsersAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operations");
                return StatusCode(500, new { Message = "An error occurred while performing bulk operations" });
            }
        }

        #endregion

        #region Password Management

        [HttpPatch("{id}/reset-password")]
        public async Task<ActionResult<UserActionResponse>> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.ResetPasswordAsync(id, dto);

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
                _logger.LogError(ex, $"Error resetting password for user {id}");
                return StatusCode(500, new { Message = "An error occurred while resetting password" });
            }
        }

        [HttpPatch("{id}/force-password-change")]
        public async Task<ActionResult<UserActionResponse>> ForcePasswordChange(Guid id)
        {
            try
            {
                var result = await _userService.ForcePasswordChangeAsync(id);

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
                _logger.LogError(ex, $"Error forcing password change for user {id}");
                return StatusCode(500, new { Message = "An error occurred while forcing password change" });
            }
        }

        [HttpPatch("{id}/extend-password")]
        public async Task<ActionResult<UserActionResponse>> ExtendPasswordExpiry(Guid id, [FromQuery] int months = 6)
        {
            try
            {
                var result = await _userService.ExtendPasswordExpiryAsync(id, months);

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
                _logger.LogError(ex, $"Error extending password expiry for user {id}");
                return StatusCode(500, new { Message = "An error occurred while extending password expiry" });
            }
        }

        #endregion

        #region Filter Helpers

        [HttpGet("by-role/{roleName}")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersByRole(Guid roleId)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users by role {roleId}");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("by-faculty/{facultyName}")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersByFaculty(Guid facultyId)
        {
            try
            {
                var users = await _userService.GetUsersByFacultyAsync(facultyId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users by faculty {facultyId}");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("by-department/{departmentName}")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersByDepartment(Guid departmentId)
        {
            try
            {
                var users = await _userService.GetUsersByDepartmentAsync(departmentId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users by department {departmentId}");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("by-type/{userType}")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersByType(string userType)
        {
            try
            {
                var users = await _userService.GetUsersByTypeAsync(userType);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users by type {userType}");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetInactiveUsers()
        {
            try
            {
                var users = await _userService.GetInactiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inactive users");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("password-expired")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetPasswordExpiredUsers()
        {
            try
            {
                var users = await _userService.GetPasswordExpiredUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting password expired users");
                return StatusCode(500, new { Message = "An error occurred while fetching users" });
            }
        }

        #endregion

        #region Statistics & Reports

        [HttpGet("statistics")]
        public async Task<ActionResult<UserStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _userService.GetUserStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return StatusCode(500, new { Message = "An error occurred while getting statistics" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] UserFilterDto filter)
        {
            try
            {
                var excelData = await _userService.ExportUsersToExcelAsync(filter);
                var fileName = $"users_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users to Excel");
                return StatusCode(500, new { Message = "An error occurred while exporting users" });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] UserFilterDto filter)
        {
            try
            {
                var csvData = await _userService.ExportUsersToCsvAsync(filter);
                var fileName = $"users_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users to CSV");
                return StatusCode(500, new { Message = "An error occurred while exporting users" });
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateReport([FromQuery] UserFilterDto filter)
        {
            try
            {
                var report = await _userService.GenerateUsersReportAsync(filter);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user report");
                return StatusCode(500, new { Message = "An error occurred while generating report" });
            }
        }

        #endregion
    }
}
