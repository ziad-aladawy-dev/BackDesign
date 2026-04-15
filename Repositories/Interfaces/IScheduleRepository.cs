using HUP.Core.Entities.Academics;
using HUP.Core.Models;

namespace HUP.Repositories.Interfaces;

public interface IScheduleRepository : IGenericRepository<HUP.Core.Entities.Academics.Schedule>
{
    Task<IEnumerable<Schedule>> GetByStudentEnrollmentsAsync(Guid studentId);
    Task<IEnumerable<Schedule>> GetAvailableSlotsAsync();
    Task<bool> TryBookSeatAsync(Guid scheduleId);
    Task<Schedule> GetByIdWithDetailsAsync(Guid id);
}