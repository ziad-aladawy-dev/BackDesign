namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasDean { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}
