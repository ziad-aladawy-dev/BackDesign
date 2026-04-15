using HUP.Repositories.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Data;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(HupDbContext context) : base(context)
        {
        }

        public async Task<Course> GetByIdWithDetailsAsync(Guid id) {
            var course = await _context.Courses
                .Include(c => c.Prerequisite)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            return course;
        }
        public async Task<Course> GetByIdTrackingAsync(Guid id) {
            var course = await _context.Courses
                .Include(c => c.Prerequisite)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            return course;
        }

        public async Task<IEnumerable<Course>> GetAllWithDetailsAsync()
        {
            return await _context.Courses
                .Include(c => c.Prerequisite)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}