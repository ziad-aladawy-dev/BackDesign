using HUP.Core.Entities.Academics;

namespace HUP.Repositories.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course> GetByIdWithDetailsAsync(Guid id);
        Task<Course> GetByIdTrackingAsync(Guid id);
        Task<IEnumerable<Course>> GetAllWithDetailsAsync();
    }
}
