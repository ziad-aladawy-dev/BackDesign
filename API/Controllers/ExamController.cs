using System.Security.Claims;
using HUP.Application.DTOs.IdentityDtos.UserDtos;
using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Academics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using HUP.Core.Constants;

namespace HUP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet("student")]
    //[Authorize(Policy = AppPermissions.VIEW_EXAM_SCHEDULE)]
    public async Task<ActionResult<IEnumerable<Exam>>> GetStudentExamSchedule()
    {
        var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var exams = await _examService.GetStudentExamScheduleAsync(studentId);
        return Ok(exams);
    }
}
