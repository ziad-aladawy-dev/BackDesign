using HUP.Application.DTOs.AcademicDtos.FacultyDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminFacultiesController : ControllerBase
    {
        private readonly IFacultyManagementService _facultyService;
        private readonly ILogger<AdminFacultiesController> _logger;

        public AdminFacultiesController(
            IFacultyManagementService facultyService,
            ILogger<AdminFacultiesController> logger)
        {
            _facultyService = facultyService;
            _logger = logger;
        }

        #region Query Endpoints

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<FacultyListDto>>> GetFaculties(
            [FromQuery] FacultyFilterDto? filter = null)
        {
            try
            {
                var result = await _facultyService.GetFacultiesAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting faculties");
                return StatusCode(500, new { Message = "An error occurred while fetching faculties" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacultyDetailsDto>> GetFacultyById(Guid id)
        {
            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id);
                if (faculty == null)
                    return NotFound(new { Message = $"Faculty with ID {id} not found" });

                return Ok(faculty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while fetching faculty" });
            }
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetFacultiesLookup()
        {
            try
            {
                var faculties = await _facultyService.GetFacultiesLookupAsync();
                return Ok(faculties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting faculties lookup");
                return StatusCode(500, new { Message = "An error occurred while fetching faculties" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FacultyListDto>>> SearchFaculties([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { Message = "Search query is required" });

                var faculties = await _facultyService.SearchFacultiesAsync(q);
                return Ok(faculties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching faculties with query: {q}");
                return StatusCode(500, new { Message = "An error occurred while searching faculties" });
            }
        }

        #endregion

        #region Create Endpoints

        [HttpPost]
        public async Task<ActionResult<FacultyActionResponse>> CreateFaculty([FromBody] CreateFacultyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _facultyService.CreateFacultyAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetFacultyById), new { id = result.Data }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating faculty");
                return StatusCode(500, new { Message = "An error occurred while creating faculty" });
            }
        }

        #endregion

        #region Update Endpoints

        [HttpPut("{id}")]
        public async Task<ActionResult<FacultyActionResponse>> UpdateFaculty(Guid id, [FromBody] UpdateFacultyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _facultyService.UpdateFacultyAsync(id, dto);

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
                _logger.LogError(ex, $"Error updating faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while updating faculty" });
            }
        }

        #endregion

        #region Dean Management

        [HttpPatch("{id}/assign-dean/{instructorId}")]
        public async Task<ActionResult<FacultyActionResponse>> AssignDean(Guid id, Guid instructorId)
        {
            try
            {
                var result = await _facultyService.AssignDeanAsync(id, instructorId);

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
                _logger.LogError(ex, $"Error assigning dean to faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while assigning dean" });
            }
        }

        [HttpPatch("{id}/remove-dean")]
        public async Task<ActionResult<FacultyActionResponse>> RemoveDean(Guid id)
        {
            try
            {
                var result = await _facultyService.RemoveDeanAsync(id);

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
                _logger.LogError(ex, $"Error removing dean from faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while removing dean" });
            }
        }

        #endregion

        #region Status Change Endpoints

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<FacultyActionResponse>> ActivateFaculty(Guid id)
        {
            try
            {
                var result = await _facultyService.ActivateFacultyAsync(id);

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
                _logger.LogError(ex, $"Error activating faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while activating faculty" });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<FacultyActionResponse>> DeactivateFaculty(Guid id)
        {
            try
            {
                var result = await _facultyService.DeactivateFacultyAsync(id);

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
                _logger.LogError(ex, $"Error deactivating faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while deactivating faculty" });
            }
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<ActionResult<FacultyActionResponse>> SoftDeleteFaculty(Guid id)
        {
            try
            {
                var result = await _facultyService.SoftDeleteFacultyAsync(id);

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
                _logger.LogError(ex, $"Error soft deleting faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting faculty" });
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<FacultyActionResponse>> RestoreFaculty(Guid id)
        {
            try
            {
                var result = await _facultyService.RestoreFacultyAsync(id);

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
                _logger.LogError(ex, $"Error restoring faculty {id}");
                return StatusCode(500, new { Message = "An error occurred while restoring faculty" });
            }
        }

        #endregion

        #region Statistics & Reports

        [HttpGet("statistics")]
        public async Task<ActionResult<FacultyStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _facultyService.GetFacultyStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting faculty statistics");
                return StatusCode(500, new { Message = "An error occurred while getting statistics" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] FacultyFilterDto? filter = null)
        {
            try
            {
                var excelData = await _facultyService.ExportFacultiesToExcelAsync(filter);
                var fileName = $"faculties_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting faculties to Excel");
                return StatusCode(500, new { Message = "An error occurred while exporting faculties" });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] FacultyFilterDto? filter = null)
        {
            try
            {
                var csvData = await _facultyService.ExportFacultiesToCsvAsync(filter);
                var fileName = $"faculties_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting faculties to CSV");
                return StatusCode(500, new { Message = "An error occurred while exporting faculties" });
            }
        }

        #endregion
    }
}