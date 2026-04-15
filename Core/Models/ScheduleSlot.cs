using DayOfWeek = HUP.Core.Enums.DayOfWeek;

namespace HUP.Core.Models;
public class ScheduleSlot
{
    public Guid SlotId { get; set; }
    public Guid CourseOfferingId { get; set; } 
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string StaffName { get; set; }
    public string Hall { get; set; }
    public string Group { get; set; }
}