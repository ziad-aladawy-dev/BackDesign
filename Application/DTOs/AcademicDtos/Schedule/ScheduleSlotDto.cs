namespace HUP.Application.DTOs.AcademicDtos.Schedule;

public class ScheduleSlotDto
{
    public Guid SlotId { get; set; }
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
    public string DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string StaffName { get; set; }
    public string Hall { get; set; }
    public string Group { get; set; }
    public int AvailableSeats { get; set; }
}