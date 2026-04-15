namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class PopularCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
    }
}
