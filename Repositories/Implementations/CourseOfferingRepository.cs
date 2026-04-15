using HUP.Core.Entities.Academics;
using HUP.Core.Enums;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class CourseOfferingRepository : GenericRepository<CourseOffering>, ICourseOfferingRepository
    {
        public CourseOfferingRepository(HupDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<CourseOffering>> GetActiveCourseOfferingAsync(Guid departmentId, Guid semesterId)
        {
            var courseOfferings = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Where(co => co.DepartmentId == departmentId && co.SemesterId == semesterId && !co.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
            return courseOfferings;
            
        }
        public async Task<IEnumerable<CourseOffering>> GetAvailableToRegisterAsync(Guid studentId)
        {
            var availableCourses = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .Where(co => co.Semester.IsActive && !co.IsDeleted)
                .Where(co => !_context.Enrollments
                    .Where(e => e.StudentId == studentId &&
                        (e.Status == EnrollmentStatus.Completed ||
                         e.Status == EnrollmentStatus.Registered ||
                         e.Status == EnrollmentStatus.InProgress))
                    .Select(e => e.CourseOfferingId)
                    .Contains(co.Id))
                .Where(co => co.Course.PrerequisiteId == null ||
                    _context.Enrollments.Any(e => e.StudentId == studentId &&
                        e.CourseOffering.CourseId == co.Course.PrerequisiteId &&
                        e.Status == EnrollmentStatus.Completed))
                //.Where(co => co.Schedules.Any(s => !s.IsDeleted && s.AvailableSeats > 0))
                .AsNoTracking()
                .ToListAsync();

            return availableCourses;
        }
        //public async Task<IEnumerable<CourseOffering>> GetAvailableToRegisterAsync(Guid studentId)
        //{
        //    var availableCourses = await _context.CourseOfferings
        //        .Where(co => co.Semester.IsActive && !co.IsDeleted)
        //        .Where(co => !_context.Enrollments  // Exclude courses the student is already registered / done / in progress
        //                .Where(e => e.StudentId == studentId &&
        //                    (e.Status == EnrollmentStatus.Completed ||
        //                     e.Status == EnrollmentStatus.Registered ||
        //                     e.Status == EnrollmentStatus.InProgress))
        //                .Select(e => e.CourseOfferingId)
        //                .Contains(co.Id))
        //        .Where(co => co.Course.PrerequisiteId == null || // If course has no prerequisite → allowed
        //                _context.Enrollments.Any(e => e.StudentId == studentId &&
        //                                         e.CourseOfferingId == co.Course.PrerequisiteId &&
        //                                         e.Status == EnrollmentStatus.Completed)) // If has prerequisite → student must have COMPLETED it
        //        .Include(co => co.Course)
        //        .Include(co => co.Semester)
        //        .Include(co => co.Schedules)
        //        .AsNoTracking()
        //        .ToListAsync();
        //    return availableCourses;
        //}

        public async Task<CourseOffering?> GetExistingAsync(Guid courseId, Guid deptId, Guid semesterId)
        {
            var entity = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .Where(co => co.CourseId == courseId
                                                                    && co.DepartmentId == deptId
                                                                    && co.SemesterId == semesterId
                                                                    && !co.IsDeleted).AsNoTracking().FirstOrDefaultAsync();
            return entity;
        }

        public async Task<IEnumerable<CourseOffering>> GetAllWithDetailsAsync()
        {
            return await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .Where(co => !co.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CourseOffering> GetByIdWithDetailsAsync(Guid id)
        {
            var co = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsDeleted);
            return co;
        }
        public async Task<CourseOffering> GetByIdTrackingAsync(Guid id)
        {
            var co = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsDeleted);
            return co;
        }

        public async Task<CourseOffering?> GetWithSchedulesAsync(Guid id)
        {
            return await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Semester)
                .Include(co => co.Schedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsDeleted);
        }
    }
}
