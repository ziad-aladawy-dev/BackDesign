using HUP.Application.DTOs.AcademicDtos.CourseDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCoursesController : ControllerBase
    {
        private readonly ICourseManagementService _courseService;
        private readonly ILogger<AdminCoursesController> _logger;

        public AdminCoursesController(
            ICourseManagementService courseService,
            ILogger<AdminCoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        #region Query Endpoints

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<CourseListDto>>> GetCourses(
            [FromQuery] CourseFilterDto? filter = null)
        {
            try
            {
                var result = await _courseService.GetCoursesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                return StatusCode(500, new { Message = "An error occurred while fetching courses" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDetailsDto>> GetCourseById(Guid id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound(new { Message = $"Course with ID {id} not found" });

                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting course {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching course" });
            }
        }

        [HttpGet("by-department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetCoursesByDepartment(Guid departmentId)
        {
            try
            {
                var courses = await _courseService.GetCoursesByDepartmentLookupAsync(departmentId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting courses for department {departmentId}");
                return StatusCode(500, new { Message = "An error occurred while fetching courses" });
            }
        }

        [HttpGet("{id}/prerequisites")]
        public async Task<ActionResult<IEnumerable<CoursePrerequisiteDto>>> GetPrerequisiteChain(Guid id)
        {
            try
            {
                var prerequisites = await _courseService.GetPrerequisiteChainAsync(id);
                return Ok(prerequisites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting prerequisites for course {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching prerequisites" });
            }
        }

        [HttpGet("{id}/departments")]
        public async Task<ActionResult<IEnumerable<CourseDepartmentDto>>> GetCourseDepartments(Guid id)
        {
            try
            {
                var departments = await _courseService.GetCourseDepartmentsAsync(id);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting departments for course {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching departments" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> SearchCourses([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { Message = "Search query is required" });

                var courses = await _courseService.SearchCoursesAsync(q);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching courses with query: {q}");
                return StatusCode(500, new { Message = "An error occurred while searching courses" });
            }
        }

        #endregion

        #region Create Endpoints

        [HttpPost]
        public async Task<ActionResult<CourseActionResponse>> CreateCourse([FromBody] CreateCourseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseService.CreateCourseAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetCourseById), new { id = result.Data }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { Message = "An error occurred while creating course" });
            }
        }

        #endregion

        #region Update Endpoints

        [HttpPut("{id}")]
        public async Task<ActionResult<CourseActionResponse>> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseService.UpdateCourseAsync(id, dto);

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
                _logger.LogError(ex, $"Error updating course {id}");
                return StatusCode(500, new { Message = "An error occurred while updating course" });
            }
        }

        #endregion

        #region Prerequisite Management

        [HttpPatch("{id}/add-prerequisite/{prerequisiteId}")]
        public async Task<ActionResult<CourseActionResponse>> AddPrerequisite(Guid id, Guid prerequisiteId)
        {
            try
            {
                var result = await _courseService.AddPrerequisiteAsync(id, prerequisiteId);

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
                _logger.LogError(ex, $"Error adding prerequisite to course {id}");
                return StatusCode(500, new { Message = "An error occurred while adding prerequisite" });
            }
        }

        [HttpPatch("{id}/remove-prerequisite")]
        public async Task<ActionResult<CourseActionResponse>> RemovePrerequisite(Guid id)
        {
            try
            {
                var result = await _courseService.RemovePrerequisiteAsync(id);

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
                _logger.LogError(ex, $"Error removing prerequisite from course {id}");
                return StatusCode(500, new { Message = "An error occurred while removing prerequisite" });
            }
        }

        #endregion

        #region Department Management

        [HttpPost("{id}/departments")]
        public async Task<ActionResult<CourseActionResponse>> AddDepartments(Guid id, [FromBody] ManageCourseDepartmentsDto dto)
        {
            try
            {
                var result = await _courseService.AddDepartmentsAsync(id, dto);

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
                _logger.LogError(ex, $"Error adding departments to course {id}");
                return StatusCode(500, new { Message = "An error occurred while adding departments" });
            }
        }

        [HttpDelete("{id}/departments/{departmentId}")]
        public async Task<ActionResult<CourseActionResponse>> RemoveDepartment(Guid id, Guid departmentId)
        {
            try
            {
                var result = await _courseService.RemoveDepartmentAsync(id, departmentId);

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
                _logger.LogError(ex, $"Error removing department from course {id}");
                return StatusCode(500, new { Message = "An error occurred while removing department" });
            }
        }

        #endregion

        #region Status Change Endpoints

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<CourseActionResponse>> ActivateCourse(Guid id)
        {
            try
            {
                var result = await _courseService.ActivateCourseAsync(id);

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
                _logger.LogError(ex, $"Error activating course {id}");
                return StatusCode(500, new { Message = "An error occurred while activating course" });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<CourseActionResponse>> DeactivateCourse(Guid id)
        {
            try
            {
                var result = await _courseService.DeactivateCourseAsync(id);

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
                _logger.LogError(ex, $"Error deactivating course {id}");
                return StatusCode(500, new { Message = "An error occurred while deactivating course" });
            }
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<ActionResult<CourseActionResponse>> SoftDeleteCourse(Guid id)
        {
            try
            {
                var result = await _courseService.SoftDeleteCourseAsync(id);

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
                _logger.LogError(ex, $"Error soft deleting course {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting course" });
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<CourseActionResponse>> RestoreCourse(Guid id)
        {
            try
            {
                var result = await _courseService.RestoreCourseAsync(id);

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
                _logger.LogError(ex, $"Error restoring course {id}");
                return StatusCode(500, new { Message = "An error occurred while restoring course" });
            }
        }

        #endregion

        #region Statistics & Reports

        [HttpGet("statistics")]
        public async Task<ActionResult<CourseStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _courseService.GetCourseStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course statistics");
                return StatusCode(500, new { Message = "An error occurred while getting statistics" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] CourseFilterDto? filter = null)
        {
            try
            {
                var excelData = await _courseService.ExportCoursesToExcelAsync(filter);
                var fileName = $"courses_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting courses to Excel");
                return StatusCode(500, new { Message = "An error occurred while exporting courses" });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] CourseFilterDto? filter = null)
        {
            try
            {
                var csvData = await _courseService.ExportCoursesToCsvAsync(filter);
                var fileName = $"courses_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting courses to CSV");
                return StatusCode(500, new { Message = "An error occurred while exporting courses" });
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateReport([FromQuery] CourseFilterDto? filter = null)
        {
            try
            {
                var report = await _courseService.GenerateCoursesReportAsync(filter);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating course report");
                return StatusCode(500, new { Message = "An error occurred while generating report" });
            }
        }

        #endregion
    }
}
