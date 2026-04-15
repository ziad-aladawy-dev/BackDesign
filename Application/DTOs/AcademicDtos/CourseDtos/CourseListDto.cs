namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseListDto
    {
        public Guid Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? CourseNameAr { get; set; }
        public int Credits { get; set; }
        public string? PrerequisiteName { get; set; }
        public Guid? PrerequisiteId { get; set; }
        public string CourseType { get; set; }
        public int TotalOfferings { get; set; }
        public int TotalEnrollments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
