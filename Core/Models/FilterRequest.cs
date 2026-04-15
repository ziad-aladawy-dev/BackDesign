namespace HUP.Core.Models
{
    public class FilterRequest
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public Dictionary<string, string> Filters { get; set; } = new();

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public bool? IsActive { get; set; }
        public bool? IncludeDeleted { get; set; } = false;

        public Guid? FacultyId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? SemesterId { get; set; }
    }
}
