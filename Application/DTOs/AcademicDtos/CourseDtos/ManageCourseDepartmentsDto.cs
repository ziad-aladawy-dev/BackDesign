using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class ManageCourseDepartmentsDto
    {
        [Required]
        public List<Guid> DepartmentIds { get; set; } = new();
        public bool IsCompulsory { get; set; } = true;
    }
}
