namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
