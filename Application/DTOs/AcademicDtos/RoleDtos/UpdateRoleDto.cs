using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class UpdateRoleDto
    {
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
