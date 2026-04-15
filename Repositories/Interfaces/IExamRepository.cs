using HUP.Core.Entities.Academics;

namespace HUP.Repositories.Interfaces
{
    public interface IExamRepository : IGenericRepository<Exam>
    {
        Task<IEnumerable<Exam>> GetByCoursesAsync(List<Guid> courseIds);
        Task<IEnumerable<Exam>> GetAllActiveAsync();
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Exam>> GetAllWithDetailsAsync();
        Task<Exam> GetByIdWithDetailsAsync(Guid id);
        Task<Exam> GetByIdTrackingAsync(Guid id);
    }
}