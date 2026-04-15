namespace HUP.Application.DTOs.AcademicDtos.Enrollment
{
    public class EnrollmentResponseDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public decimal Grade { get; set; }
    }
}