using HUP.Core.Entities.Academics;
using HUP.Core.Enums;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(HupDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Student>> GetByFacultyAsync(Guid facultyId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                .Where(s => s.FacultyID == facultyId && !s.User.IsDeleted)
                //.Where(s => s.Department.FacultyId == facultyId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Student> GetByIdWithDetailsAsync(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                     .ThenInclude(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                //.Include(s=> s.Department.Faculty)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == id);
            return student;
        }

        public async Task<Student> GetByIdTrackingAsync(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Department)
                    .ThenInclude(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                //.Include(s=> s.Department.Faculty)
                .FirstOrDefaultAsync(s => s.UserId == id);
            return student;
        }

        public async Task<IEnumerable<Student>> GetByDepartmentAsync(Guid departmentId)
        {
            var students = await _context.Students
                .Where(s => s.DepartmentId == departmentId)
                .AsNoTracking()
                .ToListAsync();
            return students;
        }

        // ---
        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAcademicStatusAsync(Guid studentId, AcademicStatus status)
        {
            var student = await GetByIdReadOnly(studentId);
            if (student != null)
            {
                student.AcademicStatus = status;
                await UpdateAsync(student);
            }
        }
    }
}