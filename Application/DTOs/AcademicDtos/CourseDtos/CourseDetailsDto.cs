namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseDetailsDto : CourseListDto
    {
        public string? CourseNameAr { get; set; }
        public string? Description { get; set; }
        public string? PrerequisiteName { get; set; }
        public Guid? PrerequisiteId { get; set; }
        public List<CourseDepartmentDto> Departments { get; set; } = new();
        public List<CoursePrerequisiteDto> PrerequisiteChain { get; set; } = new();
        public List<CourseOfferingSummaryDto> RecentOfferings { get; set; } = new();
        public int TotalOfferings { get; set; }
        public int TotalEnrollments { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
