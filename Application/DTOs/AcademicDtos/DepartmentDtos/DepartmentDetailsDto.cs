namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentDetailsDto : DepartmentListDto
    {
        public List<DepartmentCourseDto> Courses { get; set; } = new();
        public List<DepartmentInstructorDto> Instructors { get; set; } = new();
        public int TotalCourseCredits => Courses.Sum(c => c.Credits);
        public DateTime? UpdatedAt { get; set; }
        public string? FacultyContactInfo { get; set; }
    }
}
