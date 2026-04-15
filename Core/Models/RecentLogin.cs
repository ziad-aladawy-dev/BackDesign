namespace HUP.Core.Models
{
    public class RecentLogin
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
    }
}
