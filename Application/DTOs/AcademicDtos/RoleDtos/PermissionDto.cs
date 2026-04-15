namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string? Description { get; set; }
        public bool IsAssigned { get; set; } 
        public string Category { get; set; }
    }
}
