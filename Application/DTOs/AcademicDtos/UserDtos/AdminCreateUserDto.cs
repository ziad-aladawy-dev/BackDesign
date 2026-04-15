using HUP.Core.Enums;

namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class AdminCreateUserDto
    {
        // Basic Info
        public string NationalId { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid RoleId { get; set; }
        public string UserType { get; set; }

        // Academic Info
        public Guid? FacultyId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? UniversityCode { get; set; }
        public string? AcademicTitle { get; set; }
        public AcademicStatus? AcademicStatus { get; set; }
        public int? Level { get; set; }
        public decimal? CGPA { get; set; }
        public string? ProfileImage { get; set; }

        // Personal Info
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Religion { get; set; }
        public string? Nationality { get; set; }
        public string? BirthPlace { get; set; }

        // Contact Info
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AltEmail { get; set; }
    }
}
