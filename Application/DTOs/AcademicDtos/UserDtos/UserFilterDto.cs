using HUP.Core.Enums;

namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public List<Guid>? RoleIds { get; set; }
        public List<string>? UserTypes { get; set; }
        public List<Guid>? FacultyIds { get; set; }
        public List<Guid>? DepartmentIds { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsPasswordExpired { get; set; }
        public AcademicStatus? AcademicStatus { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}
