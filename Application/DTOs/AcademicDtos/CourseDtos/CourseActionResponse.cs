namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
