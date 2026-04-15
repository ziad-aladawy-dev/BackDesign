namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class UserInRoleDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
