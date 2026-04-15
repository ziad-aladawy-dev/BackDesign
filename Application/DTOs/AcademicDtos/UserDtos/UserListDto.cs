namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string NationalId { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string UserType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime PasswordExpiryDate { get; set; }
        public bool IsPasswordExpired => PasswordExpiryDate < DateTime.UtcNow;
    }
}
