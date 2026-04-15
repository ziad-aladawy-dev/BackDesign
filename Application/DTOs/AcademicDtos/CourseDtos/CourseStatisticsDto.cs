namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseStatisticsDto
    {
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int InactiveCourses { get; set; }
        public Dictionary<string, int> CoursesByType { get; set; } = new();
        public int CoursesWithPrerequisites { get; set; }
        public int CoursesWithoutPrerequisites { get; set; }
        public int TotalEnrollments { get; set; }
        public double AverageCredits { get; set; }
        public List<PopularCourseDto> PopularCourses { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
