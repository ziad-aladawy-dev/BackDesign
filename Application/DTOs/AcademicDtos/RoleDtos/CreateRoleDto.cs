using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public List<Guid> PermissionIds { get; set; } = new();
    }
}
