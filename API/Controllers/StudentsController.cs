using HUP.Application.DTOs.AcademicDtos;
using HUP.Application.DTOs.AcademicDtos.Enrollment;
using HUP.Application.DTOs.AcademicDtos.Student;
using HUP.Application.Services.Interfaces;
using HUP.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HUP.Core.Constants;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        //TODO
        //ADD UPDATE PROFILE, REGISTER STUDENT, SOFT DELETE
        
        [HttpPost]
        //[Authorize(Policy = AppPermissions.CREATE_STUDENT)]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto createDto)
        {
            await _studentService.AddStudent(createDto);
            return Ok("Added");
        }

        [HttpGet("Profile")]
        //[Authorize(Policy = AppPermissions.VIEW_PROFILE)]
        public async Task<ActionResult<StudentProfileDto>> GetProfile([FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var id = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var profile = await _studentService.GetStudentProfile(id, lang);
            if (profile == null)
                return BadRequest("User not found.");
            return Ok(profile);
        }

        [HttpPatch("Status")]
        //[Authorize(Policy = AppPermissions.UPDATE_STUDENT_STATUS)]
        public async Task<IActionResult> UpdateAcademicStatus([FromBody] StudentStatusDto statusDto)
        {
            bool result = await _studentService.UpdateStudentStatus(statusDto);
            if (!result) return BadRequest("Failed to update status");
            return Ok("Status updated successfully.");
        }

        [HttpPost("photo")]
        //[Authorize(Policy = AppPermissions.UPDATE_PROFILE)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var photoUrl = await _studentService.UploadProfilePhotoAsync(studentId, file);
            return Ok(new { Url = photoUrl });
        }

    }
}
