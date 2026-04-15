using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public AdminLookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("faculties")]
        public async Task<ActionResult<IEnumerable<FacultyLookupDto>>> GetFaculties()
        {
            var faculties = await _lookupService.GetAllFacultiesAsync();
            return Ok(faculties);
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentLookupDto>>> GetDepartments([FromQuery] Guid? facultyId = null)
        {
            if (facultyId.HasValue)
            {
                var depts = await _lookupService.GetDepartmentsByFacultyAsync(facultyId.Value);
                return Ok(depts);
            }

            var allDepts = await _lookupService.GetAllDepartmentsAsync();
            return Ok(allDepts);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleLookupDto>>> GetRoles()
        {
            var roles = await _lookupService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("user-types")]
        public ActionResult<IEnumerable<string>> GetUserTypes()
        {
            var types = _lookupService.GetAllUserTypesAsync().Result;
            return Ok(types);
        }

        [HttpGet("academic-statuses")]
        public ActionResult<IEnumerable<string>> GetAcademicStatuses()
        {
            var statuses = _lookupService.GetAllAcademicStatusesAsync().Result
                .Select(s => s.ToString());
            return Ok(statuses);
        }
    }
}
