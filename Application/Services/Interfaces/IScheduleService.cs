using HUP.Application.DTOs.AcademicDtos.Schedule;

namespace HUP.Application.Services.Interfaces
{
    public interface IScheduleService
    {
        Task Create(ScheduleSlotCreateDto createDto);
        Task Update(ScheduleSlotCreateDto createDto);
        Task SoftDelete(Guid id);
        Task Remove(Guid id);
        Task<IEnumerable<ScheduleSlotDto>> GetSlotsByStudentEnrollments(Guid studentId, string lang);
        Task<IEnumerable<ScheduleSlotDto>> GetAvailableScheduleForEnrollment(Guid studentId, string lang);
    }
}