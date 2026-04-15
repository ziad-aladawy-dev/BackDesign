namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyDepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? HeadOfDepartment { get; set; }
        public int StudentCount { get; set; }
        public int InstructorCount { get; set; }
        public bool IsActive { get; set; }
    }
}
