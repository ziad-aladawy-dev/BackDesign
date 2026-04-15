using HUP.Repositories.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums;
using HUP.Data;
using Microsoft.EntityFrameworkCore;
using HUP.Core.Models;

namespace HUP.Repositories.Implementations
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(HupDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Enrollment>> GetByStudentId(Guid studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Include(e => e.CourseOffering)
                .Include(e => e.Student)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetbySemester(Semester semester, Guid studentId)
        {
            return await _context.Enrollments
                .Where(e => e.EnrollmentDate >= semester.StartDate && e.EnrollmentDate <= semester.EndDate
                && e.StudentId == studentId && !e.IsDeleted)
                .Include(e => e.CourseOffering)
                .Include(e => e.Student)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Enrollment?> GetExistingAsync(Guid studentId, Guid courseId)
        {
            return await _context.Enrollments.Where(e => e.StudentId == studentId
                                                         && e.CourseOfferingId == courseId && !e.IsDeleted)
                .FirstOrDefaultAsync(); 
        }

        public async Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.CourseOffering)
                .Include(e => e.Student)
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Enrollment> GetByIdWithDetailsAsync(Guid id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.Schedule)
                .Include(e => e.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            return enrollment;
        }
        public async Task<Enrollment> GetByIdTrackingAsync(Guid id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.Schedule)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            return enrollment;
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentAndSemesterAsync(Guid studentId, string semester)
        {
            return await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Schedules) // Needed for conflict check in Service
                .Include(e => e.Schedule)
                .Where(e => e.StudentId == studentId && e.CourseOffering.Semester.SemesterName == semester)
                .ToListAsync();
        }

        public async Task<List<SemesterGrades>> GetStudentSemesterGradeModelsAsync(Guid studentId)
        {
            return await _context.Enrollments
                .Include(e => e.CourseOffering)
                .Where(e => e.StudentId == studentId)
                .Select(e => new SemesterGrades
                {
                    SemesterId = e.CourseOffering.Semester.Id,
                    SemesterName = e.CourseOffering.Semester.SemesterName,

                    CourseId = e.CourseOffering.CourseId,
                    CourseName = e.CourseOffering.Course.CourseName,
                    CourseCode = e.CourseOffering.Course.CourseCode,
                    CourseCredits = e.CourseOffering.Course.Credits,

                    ClassGrade = e.ClassGrade,
                    MidtermGrade = e.MidtermGrade,
                    FinalGrade = e.finalGrade
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetFilteredAsync(Guid studentId, EnrollmentFilterDto filter)
        {
            var query = _context.Enrollments
                .Where(e => e.StudentId == studentId);                

            if (filter.Status.HasValue)
                query = query.Where(e => e.Status == filter.Status);

            query = query.Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course);

            return await query.ToListAsync();
        }

        public async Task<bool> HasPassedPrerequisiteAsync(Guid studentId, Guid prerequisiteCourseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId
                               && e.CourseOffering.CourseId == prerequisiteCourseId
                               && e.Status == EnrollmentStatus.Completed
                               && e.finalGrade >= 50); // Assuming 50 is pass, or check status only if business logic dictates
        }
    }
}
