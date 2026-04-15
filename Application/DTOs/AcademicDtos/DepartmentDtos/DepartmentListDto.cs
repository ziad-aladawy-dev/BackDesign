namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentListDto
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; }
        public string BaseDepartmentCode { get; set; }
        public List<DepartmentFacultyInfoDto> Faculties { get; set; } = new();
        public string? HeadOfDepartmentName { get; set; }
        public Guid? HeadOfDepartmentId { get; set; }
        public int DurationInYears { get; set; }
        public int CompulsoryHours { get; set; }
        public int ElectiveHours { get; set; }
        public int TotalHours => CompulsoryHours + ElectiveHours;
        public int StudentCount { get; set; }
        public int InstructorCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
