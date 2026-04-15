namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class UserActivityLogDto
    {
        public Guid LogId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
