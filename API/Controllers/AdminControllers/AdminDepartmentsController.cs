using HUP.Application.DTOs.AcademicDtos.DepartmentDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDepartmentsController : ControllerBase
    {
        private readonly IDepartmentManagementService _departmentService;
        private readonly ILogger<AdminDepartmentsController> _logger;

        public AdminDepartmentsController(
            IDepartmentManagementService departmentService,
            ILogger<AdminDepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        #region Query Endpoints

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<DepartmentListDto>>> GetDepartments(
            [FromQuery] DepartmentFilterDto? filter = null)
        {
            try
            {
                var result = await _departmentService.GetDepartmentsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return StatusCode(500, new { Message = "An error occurred while fetching departments" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDetailsDto>> GetDepartmentById(Guid id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                    return NotFound(new { Message = $"Department with ID {id} not found" });

                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting department {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching department" });
            }
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetDepartmentsLookup(
            [FromQuery] Guid? facultyId = null)
        {
            try
            {
                var departments = await _departmentService.GetDepartmentsLookupAsync(facultyId);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments lookup");
                return StatusCode(500, new { Message = "An error occurred while fetching departments" });
            }
        }

        [HttpGet("codes")]
        public async Task<ActionResult<IEnumerable<string>>> GetDepartmentCodes()
        {
            try
            {
                var codes = await _departmentService.GetDepartmentCodesAsync();
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department codes");
                return StatusCode(500, new { Message = "An error occurred while fetching department codes" });
            }
        }

        [HttpGet("by-faculty/{facultyId}")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetDepartmentsByFaculty(Guid facultyId)
        {
            try
            {
                var departments = await _departmentService.GetDepartmentsByFacultyLookupAsync(facultyId);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting departments for faculty {facultyId}");
                return StatusCode(500, new { Message = "An error occurred while fetching departments" });
            }
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<IEnumerable<DepartmentCourseDto>>> GetDepartmentCourses(Guid id)
        {
            try
            {
                var courses = await _departmentService.GetDepartmentCoursesAsync(id);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting courses for department {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching courses" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DepartmentListDto>>> SearchDepartments([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { Message = "Search query is required" });

                var departments = await _departmentService.SearchDepartmentsAsync(q);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching departments with query: {q}");
                return StatusCode(500, new { Message = "An error occurred while searching departments" });
            }
        }

        #endregion

        #region Create Endpoints

        [HttpPost]
        public async Task<ActionResult<DepartmentActionResponse>> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _departmentService.CreateDepartmentAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Data }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, new { Message = "An error occurred while creating department" });
            }
        }

        #endregion

        #region Update Endpoints

        [HttpPut("{id}")]
        public async Task<ActionResult<DepartmentActionResponse>> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _departmentService.UpdateDepartmentAsync(id, dto);

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
                _logger.LogError(ex, $"Error updating department {id}");
                return StatusCode(500, new { Message = "An error occurred while updating department" });
            }
        }

        #endregion

        #region Head of Department Management

        [HttpPatch("{id}/assign-head/{instructorId}")]
        public async Task<ActionResult<DepartmentActionResponse>> AssignHeadOfDepartment(Guid id, Guid instructorId)
        {
            try
            {
                var result = await _departmentService.AssignHeadOfDepartmentAsync(id, instructorId);

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
                _logger.LogError(ex, $"Error assigning head to department {id}");
                return StatusCode(500, new { Message = "An error occurred while assigning head" });
            }
        }

        [HttpPatch("{id}/remove-head")]
        public async Task<ActionResult<DepartmentActionResponse>> RemoveHeadOfDepartment(Guid id)
        {
            try
            {
                var result = await _departmentService.RemoveHeadOfDepartmentAsync(id);

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
                _logger.LogError(ex, $"Error removing head from department {id}");
                return StatusCode(500, new { Message = "An error occurred while removing head" });
            }
        }

        #endregion

        #region Course Management

        [HttpPost("{id}/courses")]
        public async Task<ActionResult<DepartmentActionResponse>> AddCourseToProgram(Guid id, [FromBody] ManageDepartmentCourseDto dto)
        {
            try
            {
                var result = await _departmentService.AddCourseToProgramAsync(id, dto);

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
                _logger.LogError(ex, $"Error adding course to department {id}");
                return StatusCode(500, new { Message = "An error occurred while adding course" });
            }
        }

        [HttpDelete("{id}/courses/{courseId}")]
        public async Task<ActionResult<DepartmentActionResponse>> RemoveCourseFromProgram(Guid id, Guid courseId)
        {
            try
            {
                var result = await _departmentService.RemoveCourseFromProgramAsync(id, courseId);

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
                _logger.LogError(ex, $"Error removing course from department {id}");
                return StatusCode(500, new { Message = "An error occurred while removing course" });
            }
        }

        #endregion

        #region Transfer Operations

        [HttpPost("transfer")]
        public async Task<ActionResult<DepartmentActionResponse>> TransferItems([FromBody] TransferDepartmentItemsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _departmentService.TransferItemsAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring department items");
                return StatusCode(500, new { Message = "An error occurred while transferring items" });
            }
        }

        #endregion

        #region Status Change Endpoints

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<DepartmentActionResponse>> ActivateDepartment(Guid id)
        {
            try
            {
                var result = await _departmentService.ActivateDepartmentAsync(id);

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
                _logger.LogError(ex, $"Error activating department {id}");
                return StatusCode(500, new { Message = "An error occurred while activating department" });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<DepartmentActionResponse>> DeactivateDepartment(Guid id)
        {
            try
            {
                var result = await _departmentService.DeactivateDepartmentAsync(id);

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
                _logger.LogError(ex, $"Error deactivating department {id}");
                return StatusCode(500, new { Message = "An error occurred while deactivating department" });
            }
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<ActionResult<DepartmentActionResponse>> SoftDeleteDepartment(Guid id)
        {
            try
            {
                var result = await _departmentService.SoftDeleteDepartmentAsync(id);

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
                _logger.LogError(ex, $"Error soft deleting department {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting department" });
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<DepartmentActionResponse>> RestoreDepartment(Guid id)
        {
            try
            {
                var result = await _departmentService.RestoreDepartmentAsync(id);

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
                _logger.LogError(ex, $"Error restoring department {id}");
                return StatusCode(500, new { Message = "An error occurred while restoring department" });
            }
        }

        #endregion

        #region Statistics & Reports

        [HttpGet("statistics")]
        public async Task<ActionResult<DepartmentStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _departmentService.GetDepartmentStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department statistics");
                return StatusCode(500, new { Message = "An error occurred while getting statistics" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] DepartmentFilterDto? filter = null)
        {
            try
            {
                var excelData = await _departmentService.ExportDepartmentsToExcelAsync(filter);
                var fileName = $"departments_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting departments to Excel");
                return StatusCode(500, new { Message = "An error occurred while exporting departments" });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] DepartmentFilterDto? filter = null)
        {
            try
            {
                var csvData = await _departmentService.ExportDepartmentsToCsvAsync(filter);
                var fileName = $"departments_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting departments to CSV");
                return StatusCode(500, new { Message = "An error occurred while exporting departments" });
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateReport([FromQuery] DepartmentFilterDto? filter = null)
        {
            try
            {
                var report = await _departmentService.GenerateDepartmentsReportAsync(filter);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating department report");
                return StatusCode(500, new { Message = "An error occurred while generating report" });
            }
        }

        #endregion
    }
}
