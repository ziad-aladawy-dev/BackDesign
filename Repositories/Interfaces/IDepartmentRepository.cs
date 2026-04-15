using HUP.Core.Entities.Academics;

namespace HUP.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<IEnumerable<Department>> GetByFacultyIdAsync(Guid facultyId);
        Task<IEnumerable<Department>> GetAllWithDetailsAsync();
        Task<Department> GetByIdWithDetailsAsync(Guid id);
        Task<Department> GetByIdTrackingAsync(Guid id);
    }
}