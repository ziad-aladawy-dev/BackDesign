namespace HUP.Core.Models
{
    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int NewUsers { get; set; }
        public int ActiveSessions { get; set; }
        public int Enrollments { get; set; }
        public int Logins { get; set; }
    }
}
