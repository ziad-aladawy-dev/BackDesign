using HUP.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class UpdateCourseDto
    {
        [StringLength(20)]
        public string? CourseCode { get; set; }

        [StringLength(200)]
        public string? CourseName { get; set; }

        [StringLength(200)]
        public string? CourseNameAr { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 6)]
        public int? Credits { get; set; }

        public Guid? PrerequisiteId { get; set; }

        public CourseType? CourseType { get; set; }

        public List<Guid>? DepartmentIds { get; set; }

        public bool? IsCompulsory { get; set; }

        public bool? IsActive { get; set; }
    }
}
