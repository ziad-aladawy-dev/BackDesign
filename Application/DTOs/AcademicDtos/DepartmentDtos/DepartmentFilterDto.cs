namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentFilterDto
    {
        public string? SearchTerm { get; set; }

        public List<Guid>? FacultyIds { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public bool? HasHead { get; set; }

        public bool? IsActive { get; set; }

        public int? MinStudents { get; set; }

        public int? MinInstructors { get; set; }

        public int? MinTotalHours { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DepartmentName";
        public string? SortOrder { get; set; } = "asc";
    }
}
