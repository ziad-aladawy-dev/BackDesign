namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentCourseDto
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public int Level { get; set; }
        public bool IsCompulsory { get; set; }
        public string? PrerequisiteName { get; set; }
        public int StudentEnrollmentCount { get; set; }
    }
}
