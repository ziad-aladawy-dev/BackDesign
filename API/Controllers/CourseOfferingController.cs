using System.Security.Claims;
using HUP.Application.DTOs.AcademicDtos.CourseOffering;
using Microsoft.AspNetCore.Mvc;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using HUP.Core.Constants;

namespace HUP.API.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class CourseOfferingController : ControllerBase
   {
       private readonly ICourseOfferingService _service;

       public CourseOfferingController(ICourseOfferingService service)
       {
           _service = service;
       }
       
        //TODO
       //ADD UPDATE, MODIFY DELETE

       // GET: api/CourseOffering
       [HttpGet]
       //[Authorize(Policy = AppPermissions.VIEW_COURSE_OFFERING)]
       public async Task<ActionResult<IEnumerable<CourseOfferingDto>>> GetAll([FromHeader(Name = "Accept-Language")] string lang = "ar")
       {
           var courseOfferings = await _service.GetAllAsync(lang);
           return Ok(courseOfferings);
       }

       // GET: api/CourseOffering/{id}
       [HttpGet("{id}")]
       //[Authorize(Policy = AppPermissions.VIEW_COURSE_OFFERING)]
       public async Task<ActionResult<CourseOfferingDto>> GetById(Guid id, [FromHeader(Name = "Accept-Language")] string lang = "ar")
       {
           var courseOffering = await _service.GetByIdAsync(id, lang);
           if (courseOffering == null)
           {
               return NotFound();
           }
           return Ok(courseOffering);
       }

       // GET: api/CourseOffering/active/{departmentId}/{semesterId}
       [HttpGet("active/{departmentId}/{semesterId}")]
       //[Authorize(Policy = AppPermissions.VIEW_COURSE_OFFERING)]
       public async Task<ActionResult<IEnumerable<CourseOfferingDto>>> GetActiveCourseOfferings(Guid departmentId, Guid semesterId
           , [FromHeader(Name = "Accept-Language")] string lang = "ar" )
       {
           var courseOfferings = await _service.GetActiveCourseOfferingAsync(departmentId, semesterId
           ,lang);
           return Ok(courseOfferings);
       }

       // GET: api/CourseOffering/available/{studentId}
       [HttpGet("available/")]
       //[Authorize(Policy = AppPermissions.VIEW_COURSE_OFFERING)]
       public async Task<ActionResult<IEnumerable<CourseOfferingDto>>> GetAvailableToRegister([FromHeader(Name = "Accept-Language")] string lang = "ar")
       {
           var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
           var courseOfferings = await _service.GetAvailableToRegisterAsync(studentId, lang);
           return Ok(courseOfferings);
       }

       // POST: api/CourseOffering
       [HttpPost]
       //[Authorize(Policy = AppPermissions.CREATE_COURSE_OFFERING)]
       public async Task<ActionResult<CourseOfferingDto>> Create([FromBody] CreateCourseOfferingDto createDto)
       {
           var exist = await _service.Exists(createDto);
           if (exist)
               return BadRequest("Course offering already exists");
           await _service.AddAsync(createDto);
           return Ok("Created Successfully");
       }

       // PUT: api/CourseOffering/{id}
       [HttpPut("{id}")]
       //[Authorize(Policy = AppPermissions.UPDATE_COURSE_OFFERING)]
       public async Task<IActionResult> Update(Guid id, [FromBody] CreateCourseOfferingDto updateDto)
       {
           // await _service.Update(id, updateDto);
           return NoContent();
       }

       // DELETE: api/CourseOffering/{id}
       [HttpDelete("{id}")]
       //[Authorize(Policy = AppPermissions.DELETE_COURSE_OFFERING)]
       public async Task<IActionResult> Delete(Guid id)
       {
           await _service.SoftDelete(id);
           return NoContent();
       }

       // DELETE: api/CourseOffering/{id}/hard
       [HttpDelete("{id}/hard")]
       //[Authorize(Policy = AppPermissions.DELETE_COURSE_OFFERING)]
       public async Task<IActionResult> HardDelete(Guid id)
       {
           await _service.Remove(id);
           return NoContent();
       }
   }
}
