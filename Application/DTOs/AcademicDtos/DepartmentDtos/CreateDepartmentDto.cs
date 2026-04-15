using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class CreateDepartmentDto
    {
        [Required]
        [StringLength(200)]
        public string DepartmentName { get; set; }

        [Required]
        [StringLength(20)]
        public string BaseDepartmentCode { get; set; }

        [Required]
        public List<Guid> FacultyIds { get; set; } = new();

        public Guid? HeadOfDepartmentId { get; set; }

        [Range(0, 200)]
        public int CompulsoryHours { get; set; }

        [Range(0, 200)]
        public int ElectiveHours { get; set; }
    }
}
