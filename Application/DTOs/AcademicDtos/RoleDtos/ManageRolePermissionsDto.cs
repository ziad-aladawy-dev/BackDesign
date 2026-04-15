using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class ManageRolePermissionsDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
