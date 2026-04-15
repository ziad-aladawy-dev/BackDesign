namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class ProfileStatusDto
    {
        public bool PasswordExpired { get; set; }
        public bool ProfileIncomplete { get; set; }
        public List<string> MissingFields { get; set; } = new();
    }
}
