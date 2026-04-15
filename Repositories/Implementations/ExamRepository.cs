using HUP.Core.Entities.Academics;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class ExamRepository : GenericRepository<Exam>, IExamRepository
    {
        public ExamRepository(HupDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Exam>> GetAllWithDetailsAsync()
        {
            return await _context.Exams
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<Exam> GetByIdWithDetailsAsync(Guid id)
        {
            var exam = _context.Exams.Include(e => e.CourseOffering)
                .ThenInclude(c => c.Department)
                .ThenInclude(d => d.StaffMembers)
                .ThenInclude(i => i.User)
                .AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return exam;
        }
        public Task<Exam> GetByIdTrackingAsync(Guid id)
        {
            var exam = _context.Exams.Include(e => e.CourseOffering)
                .ThenInclude(c => c.Department)
                .ThenInclude(d => d.StaffMembers)
                .ThenInclude(i => i.User)
                .AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return exam;
        }

        public async Task<IEnumerable<Exam>> GetByCoursesAsync(List<Guid> courseIds)
        {
            if (!courseIds.Any())
                return new List<Exam>();

            return await _context.Exams
                .Include(e => e.CourseOffering)
                .Where(e => courseIds.Contains(e.CourseOffering.CourseId))
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.ExamTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exam>> GetAllActiveAsync()
        {
            return await _context.Exams
                .Include(e => e.CourseOffering)
                .ThenInclude(c => c.Department)
                .ThenInclude(d => d.StaffMembers)
                .ThenInclude(i => i.User)
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.ExamTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(Exam exam)
        {
            exam.UpdatedAt = DateTime.UtcNow;
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var exam = await GetByIdWithDetailsAsync(id);
            if (exam != null)
            {
                exam.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(exam);
            }
        }
    }
}