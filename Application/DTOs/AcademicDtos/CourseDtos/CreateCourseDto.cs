using HUP.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CreateCourseDto
    {
        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CourseNameAr { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 6)]
        public int Credits { get; set; }

        public Guid? PrerequisiteId { get; set; }

        public CourseType CourseType { get; set; } = CourseType.Specialized;

        [Required]
        public List<Guid> DepartmentIds { get; set; } = new();

        public bool IsCompulsory { get; set; } = true;
    }
}
