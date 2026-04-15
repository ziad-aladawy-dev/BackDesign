namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class UserDetailsDto : UserListDto
    {
        public Guid? FacultyId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }

        // Personal Info
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Religion { get; set; }
        public string Nationality { get; set; }
        public string BirthPlace { get; set; }

        // Contact Info
        public string Address { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string AltEmail { get; set; }

        // Academic Info
        public string UniversityCode { get; set; }
        public string UniversityEmail { get; set; }
        public string AcademicStatus { get; set; }
        public int? Level { get; set; }
        public decimal? CGPA { get; set; }
        public string ProfileImage { get; set; }

        public string AcademicTitle { get; set; }

        // Audit Info
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}
