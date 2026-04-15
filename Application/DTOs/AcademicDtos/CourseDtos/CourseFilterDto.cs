using HUP.Core.Enums;

namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseFilterDto
    {
        public string? SearchTerm { get; set; }

        public List<Guid>? DepartmentIds { get; set; }

        public int? MinCredits { get; set; }
        public int? MaxCredits { get; set; }

        public bool? HasPrerequisite { get; set; }

        public bool? IsActive { get; set; }

        public CourseType? CourseType { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CourseCode";
        public string? SortOrder { get; set; } = "asc";
    }
}
