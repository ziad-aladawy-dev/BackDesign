using HUP.Core.Entities.Shared;
using DayOfWeek = HUP.Core.Enums.DayOfWeek;

namespace HUP.Core.Entities.Academics
{
    public class Schedule : BaseEntity
    {
        public Guid CourseOfferingId { get; set; }
        public Guid StaffId { get; set; }
        public string Group { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Hall  { get; set; }
        public string StaffName { get; set; } // Can keep for snapshot or remove if now redundant
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public CourseOffering CourseOffering { get; set; }
        public Staff Staff { get; set; }
    }
}