using System.Security.Claims;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HUP.Core.Constants;

namespace HUP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramPlanController : ControllerBase
    {
        private readonly IProgramPlanService _programPlanService;

        public ProgramPlanController(IProgramPlanService programPlanService)
        {
            _programPlanService = programPlanService;
        }
        //TODO
        // ADD LOCALIZATION
        [HttpGet("student/")]
        //[Authorize(Policy = AppPermissions.VIEW_PROGRAM_PLAN)]
        public async Task<IActionResult> GetProgramPlanByStudentId([FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _programPlanService.GetByDepartmentAsync(studentId, lang);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
