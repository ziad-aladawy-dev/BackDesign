using System.Security.Claims;
using HUP.Application.DTOs.FinancialDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HUP.Core.Constants;

namespace HUP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public FinancialController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet("fees")]
        //[Authorize(Policy = AppPermissions.VIEW_FEES)]
        public async Task<ActionResult<FinancialSummaryDto>> GetMyFees([FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var summary = await _financialService.GetStudentFinancialSummaryAsync(studentId, lang);
            return Ok(summary);
        }
    }
}
