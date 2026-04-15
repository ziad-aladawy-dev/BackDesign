using HUP.Core.Entities.Identity;
using HUP.Core.Models;

namespace HUP.Application.DTOs.IdentityDtos.UserDtos;

public class CreateUserDto
{
    public string NationalId { get; set; }
    public string? Email { get; set; }
    public string PasswordHash { get; set; }
    public LocalizedText FullName { get; set; }
    public Guid RoleId { get; set; }
    public PersonalInfoDto PersonalInfo { get; set; }
    public ContactInfoDto ContactInfo { get; set; }
}