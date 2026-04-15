namespace HUP.Core.Models
{
    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int SuspendedStudents { get; set; }
        public int NewStudentsThisSemester { get; set; }

        public int TotalInstructors { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalDepartmentHeads { get; set; }

        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int CourseOfferingsThisSemester { get; set; }

        public int TotalEnrollments { get; set; }
        public int CurrentSemesterEnrollments { get; set; }

        public int TotalFaculties { get; set; }
        public int TotalDepartments { get; set; }

        public Dictionary<string, int> UsersByRole { get; set; }
        public Dictionary<string, int> StudentsByFaculty { get; set; }
        public Dictionary<string, int> StudentsByLevel { get; set; }
        public Dictionary<string, int> CoursesByType { get; set; }

        public List<RecentActivity> RecentActivities { get; set; }

        public List<RecentLogin> RecentLogins { get; set; }

        public List<DailyStats> Last30DaysStats { get; set; }
    }
}
