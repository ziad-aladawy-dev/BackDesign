using System.Security.Claims;
using HUP.Application.DTOs.IdentityDtos.UserDtos;
using HUP.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HUP.Core.Constants;

namespace HUP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService service)
    {
        _userService = service;
    }

    //GET : api/User
    //TODO
    // (ADD PAGINATION + FILTERS)
    [HttpGet]
    //[Authorize(Policy = AppPermissions.VIEW_USERS)]
    public async Task<ActionResult<IEnumerable<UsersListResponse>>> GetAll([FromHeader(Name = "Accept-Language")] string lang = "ar")
    {
        var users = await _userService.GetAllUsers(lang);
        return Ok(users);
    }
    //Get : api/User/{id}
    [HttpGet("{id}")]
    //[Authorize(Policy = AppPermissions.VIEW_PROFILE)]
    public async Task<ActionResult<ProfileInfoDto>> GetById(Guid id, [FromHeader(Name = "Accept-Language")] string lang = "ar")
    {
        var profile = await _userService.GetUserById(id, lang);
        if (profile == null)
            return BadRequest("User not found.");
        return Ok(profile);
    }

    [HttpPatch("insert-profile-data")]
    //[Authorize(Policy = AppPermissions.UPDATE_PROFILE)]
    public async Task<IActionResult> InsertMissingData([FromBody] UpdateInfoDto dto)
    {
        Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        bool result = await _userService.InsertMissingData(userId, dto);
        if (!result) return BadRequest("Failed to update");
        return Ok("Profile updated successfully.");
    }

    [HttpPatch("update-profile")]
    //[Authorize(Policy = AppPermissions.UPDATE_PROFILE)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateInfoDto dto)
    {
        Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        bool result = await _userService.Update(userId, dto);
        if (!result) return BadRequest("Failed to update");
        return Ok("Profile updated successfully.");
    }

    [HttpPost]
    //[Authorize(Policy = AppPermissions.CREATE_USER)]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto dto)
    {
        var exist = await _userService.Exists(dto.NationalId);
        if (exist) return BadRequest("This national Id is already registered");
        await _userService.AddAsync(dto);
        return Ok("User added successfully.");
    }
    
    [HttpDelete("{id}")]
    //[Authorize(Policy = AppPermissions.DELETE_USER)]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        var user = await _userService.SoftDelete(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
    
    [HttpDelete("{id}/hard")]
    //[Authorize(Policy = AppPermissions.DELETE_USER)]
    public async Task<IActionResult> HardDelete(Guid id)
    {
        await _userService.Remove(id);
        return NoContent();
    }
}
