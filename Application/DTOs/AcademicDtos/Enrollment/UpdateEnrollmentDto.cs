using HUP.Core.Enums;

namespace HUP.Application.DTOs.AcademicDtos.Enrollment
{
    public class UpdateEnrollmentDto
    {
        public decimal? ClassGrade { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? finalGrade {get; set; }
        public EnrollmentStatus Status { get; set; }
    }
}