using HUP.Core.Entities.Academics;

namespace HUP.Repositories.Interfaces;

public interface IFacultyRepository : IGenericRepository<Faculty>
{
    Task<IEnumerable<Faculty>> GetAllWithDetailsAsync();
    void Update(Faculty entity);
    void SoftDelete(Guid id);
}