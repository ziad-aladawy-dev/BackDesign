using HUP.Application.Services.Interfaces;
using HUP.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentsController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        //[Authorize(Policy = AppPermissions.VIEW_DEPARTMENTS)]
        public async Task<IActionResult> GetAll([FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var result = await _service.GetAllAsync(lang);
            return Ok(result);
        }

        [HttpGet("{id}")]
        //[Authorize(Policy = AppPermissions.VIEW_DEPARTMENTS)]
        public async Task<IActionResult> GetById(Guid id, [FromHeader(Name = "Accept-Language")] string lang = "ar")
        {
            var result = await _service.GetByIdAsync(id, lang);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
