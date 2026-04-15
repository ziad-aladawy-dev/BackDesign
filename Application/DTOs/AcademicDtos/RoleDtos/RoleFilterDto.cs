namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class RoleFilterDto
    {
        public string? SearchTerm { get; set; } 

        public bool? IsActive { get; set; }

        public int? MinUsers { get; set; } 

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DisplayName";
        public string? SortOrder { get; set; } = "asc";
    }
}
