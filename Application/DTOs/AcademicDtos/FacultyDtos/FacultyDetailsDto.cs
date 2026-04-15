namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyDetailsDto : FacultyListDto
    {
        public List<FacultyDepartmentDto> Departments { get; set; } = new();
        public int InstructorCount { get; set; }
        public int CourseCount { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
