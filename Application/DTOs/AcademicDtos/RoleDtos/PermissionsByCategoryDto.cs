namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class PermissionsByCategoryDto
    {
        public string Category { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
