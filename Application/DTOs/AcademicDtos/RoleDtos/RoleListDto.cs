namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class RoleListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string? Description { get; set; }
        public int UsersCount { get; set; }
        public int PermissionsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }
}
