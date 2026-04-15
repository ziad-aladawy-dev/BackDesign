using HUP.Core.Entities.Academics;
using HUP.Core.Models;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(HupDbContext context) : base(context)
        {
        }

        public async Task<Schedule> GetByIdWithDetailsAsync(Guid id)
        {
            var s = await _context.Schedules
                .Where(s => s.Id == id)
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(s => s.Staff)
                .AsNoTracking().FirstOrDefaultAsync();
            return s;
        }

        public async Task<IEnumerable<Schedule>> GetByStudentEnrollmentsAsync(Guid studentId)
        {
            var schedules = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.CourseOffering)
                        .ThenInclude(co => co.Course)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Staff)
                        .ThenInclude(st => st.User)
                .Select(e => e.Schedule)
                .Where(s => s != null && !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return schedules;
        }

        public async Task<IEnumerable<Schedule>> GetAvailableSlotsAsync()
        {
            return await _context.Schedules
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Semester)
                .Include(s => s.Staff)
                    .ThenInclude(st => st.User)
                .Where(s => s.AvailableSeats > 0
                            && !s.IsDeleted
                            && s.CourseOffering.Semester.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> TryBookSeatAsync(Guid scheduleId)
        {
            // Raw SQL update for atomicity and concurrency control
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Schedules SET AvailableSeats = AvailableSeats - 1, UpdatedAt = GETUTCDATE() WHERE Id = {0} AND AvailableSeats > 0",
                scheduleId);

            return rowsAffected > 0;
        }
    }
}