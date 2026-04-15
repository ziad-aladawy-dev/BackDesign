namespace HUP.Application.DTOs.LookupDtos
{
    public class DepartmentLookupDto : LookupDto
    {
        public Guid FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string? DepartmentCode { get; set; }
        public bool IsPrimary { get; set; }
    }
}
