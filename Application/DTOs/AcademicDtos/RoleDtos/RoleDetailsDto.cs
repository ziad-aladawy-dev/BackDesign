namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class RoleDetailsDto : RoleListDto
    {
        public List<PermissionDto> Permissions { get; set; } = new();
        public List<UserInRoleDto> Users { get; set; } = new();
        public DateTime? UpdatedAt { get; set; }
    }
}
