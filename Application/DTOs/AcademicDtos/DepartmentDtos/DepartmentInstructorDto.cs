namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentInstructorDto
    {
        public Guid InstructorId { get; set; }
        public string FullName { get; set; }
        public string AcademicTitle { get; set; }
        public string Email { get; set; }
        public bool IsHead => IsHeadOfDepartment;
        public bool IsHeadOfDepartment { get; set; }
        public int CourseCount { get; set; }
        public bool IsActive { get; set; }
    }
}
