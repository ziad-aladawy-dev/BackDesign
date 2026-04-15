namespace HUP.Application.DTOs.AcademicDtos.CourseDtos
{
    public class CourseOfferingSummaryDto
    {
        public Guid OfferingId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public int EnrolledStudents { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
