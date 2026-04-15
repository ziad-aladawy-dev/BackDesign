namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseDepartmentDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid? FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string? FacultyCode { get; set; }
        public string? DepartmentCode { get; set; }
        public bool IsCompulsory { get; set; }
        public bool IsPrimary { get; set; }
    }
}
