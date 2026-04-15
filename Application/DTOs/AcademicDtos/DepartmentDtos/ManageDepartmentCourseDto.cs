using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class ManageDepartmentCourseDto
    {
        [Required]
        public Guid CourseId { get; set; }

        public bool IsCompulsory { get; set; } = true;

        public int? SuggestedLevel { get; set; }
    }
}
