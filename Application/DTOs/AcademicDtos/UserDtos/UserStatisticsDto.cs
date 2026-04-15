namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int DeletedUsers { get; set; }
        public int StudentsCount { get; set; }
        public int InstructorsCount { get; set; }
        public int AdminsCount { get; set; }
        public int PasswordExpiredCount { get; set; }
        public int UsersWithMissingInfo { get; set; }

        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public Dictionary<string, int> UsersByFaculty { get; set; } = new();
        public Dictionary<string, int> UsersByStatus { get; set; } = new();

        public DateTime GeneratedAt { get; set; }
    }
}
