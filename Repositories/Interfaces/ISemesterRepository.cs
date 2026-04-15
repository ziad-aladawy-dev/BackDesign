namespace HUP.Repositories.Interfaces;

public interface ISemesterRepository : IGenericRepository<HUP.Core.Entities.Academics.Semester>
{
    Task<HUP.Core.Entities.Academics.Semester?> GetActiveSemesterAsync();
}
