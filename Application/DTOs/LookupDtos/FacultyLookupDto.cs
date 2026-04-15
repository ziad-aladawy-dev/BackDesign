namespace HUP.Application.DTOs.LookupDtos
{
    public class FacultyLookupDto : LookupDto
    {
        public List<DepartmentLookupDto>? Departments { get; set; }
        public string? Code { get; set; }
    }
}
