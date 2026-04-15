using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class CopyRolePermissionsDto
    {
        [Required]
        public Guid SourceRoleId { get; set; }

        [Required]
        public Guid TargetRoleId { get; set; }

        public bool OverwriteExisting { get; set; } = false;
    }
}
