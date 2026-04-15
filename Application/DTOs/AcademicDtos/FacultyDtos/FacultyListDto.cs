namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyListDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public string? DeanName { get; set; }
        public Guid? DeanId { get; set; }

        public string? ContactInfo { get; set; }

        public int DepartmentCount { get; set; }
        public int StudentCount { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayName => Thread.CurrentThread.CurrentUICulture.Name == "ar" ? NameAr : NameEn;
    }
}
