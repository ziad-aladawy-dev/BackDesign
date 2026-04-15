using System.Security.Claims;
using HUP.Application.DTOs.AcademicDtos;
using Microsoft.AspNetCore.Mvc;
using HUP.Application.Services.Interfaces;
using HUP.Application.DTOs.AcademicDtos.Enrollment;
using Microsoft.AspNetCore.Authorization;
using HUP.Core.Constants;

namespace HUP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _service;

        public EnrollmentController(IEnrollmentService service)
        {
            _service = service;
        }
        //TODO
        // ADD LOCALIZATION
        // ADD FILTERS 

        // GET: api/Enrollment/{id}
        [HttpGet("{id}")]
        //[Authorize(Policy = AppPermissions.VIEW_ENROLLMENT)]
        public async Task<ActionResult<EnrollmentResponseDto>> GetById(Guid id,
            [FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var enrollment = await _service.GetByIdAsync(id, lang);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.StudentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return Ok(enrollment);
        }

        // POST: api/Enrollment
        [HttpPost]
        //[Authorize(Policy = AppPermissions.CREATE_ENROLLMENT)]
        public async Task<IActionResult> Create([FromBody] List<CreateEnrollmentDto> createDtos)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            foreach (var dto in createDtos)
            {
                dto.StudentId = studentId;
            }

            await _service.AddAsync(createDtos);
            return StatusCode(StatusCodes.Status201Created);
        }

        // Patch: api/Enrollment/{id}
        [HttpPatch("{id}/status")]
        //[Authorize(Policy = AppPermissions.UPDATE_ENROLLMENT)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEnrollmentDto updateDto)
        {
            await _service.Update(id, updateDto);
            return NoContent();
        }
        
        // DELETE: api/Enrollment/{id}
        [HttpDelete("{id}")]
        //[Authorize(Policy = AppPermissions.DELETE_ENROLLMENT)]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDelete(id);
            return NoContent();
        }

        // DELETE: api/Enrollment/{id}/hard
        [HttpDelete("{id}/hard")]
        //[Authorize(Policy = AppPermissions.DELETE_ENROLLMENT)]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            await _service.Remove(id);
            return NoContent();
        }

        // Get: api/Enrollment/AllGrades/{studentId}
        [HttpGet("AllGrades")]
        //[Authorize(Policy = AppPermissions.VIEW_GRADES)]
        public async Task<IActionResult> GetStudentGrades([FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _service.GetStudentGradesAsync(studentId, lang);
            return Ok(result);
        }

        // GET: api/Enrollment/Student
        [HttpGet("Student")]
        //[Authorize(Policy = AppPermissions.VIEW_ENROLLMENT)]
        public async Task<ActionResult<IEnumerable<EnrollmentResponseDto>>> GetRegisteredByStudentAsync([FromQuery] EnrollmentFilterDto filter, [FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.GetRegisteredByStudentAsync(studentId, lang, filter);
            return Ok(result);
        }
    }
}
