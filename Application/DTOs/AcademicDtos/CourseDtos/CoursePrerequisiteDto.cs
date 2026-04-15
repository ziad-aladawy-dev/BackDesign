namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CoursePrerequisiteDto
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
    }
}
