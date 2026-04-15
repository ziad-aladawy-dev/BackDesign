using HUP.Core.Entities.Academics;
using HUP.Core.Enums;

namespace HUP.Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<IEnumerable<Student>> GetByFacultyAsync(Guid facultyId);
        Task<IEnumerable<Student>> GetByDepartmentAsync(Guid departmentId);
        Task<Student> GetByIdWithDetailsAsync(Guid id);
        Task<Student> GetByIdTrackingAsync(Guid id);
    }
}
