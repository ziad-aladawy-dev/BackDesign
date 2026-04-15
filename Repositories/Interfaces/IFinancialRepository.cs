using HUP.Core.Entities.Financial;

namespace HUP.Repositories.Interfaces
{
    public interface IFinancialRepository : IGenericRepository<StudentFee>
    {
        Task<IEnumerable<StudentFee>> GetStudentFeesAsync(Guid studentId);
        Task<IEnumerable<StudentFee>> GetStudentFeesBySemesterAsync(Guid studentId, Guid semesterId);
    }
}
