using HUP.Core.Entities.Academics;

namespace HUP.Repositories.Interfaces
{
    public interface ICourseOfferingRepository : IGenericRepository<CourseOffering>
    {
        Task<IEnumerable<CourseOffering>> GetActiveCourseOfferingAsync(Guid departmentId, Guid semesterId);
        Task<IEnumerable<CourseOffering>> GetAvailableToRegisterAsync(Guid studentId);
        Task<CourseOffering?> GetExistingAsync(Guid courseId, Guid deptId, Guid semesterId);
        Task<CourseOffering?> GetWithSchedulesAsync(Guid id);
        Task<IEnumerable<CourseOffering>> GetAllWithDetailsAsync();
        Task<CourseOffering> GetByIdWithDetailsAsync(Guid id);
        Task<CourseOffering> GetByIdTrackingAsync(Guid id);
    }
}
