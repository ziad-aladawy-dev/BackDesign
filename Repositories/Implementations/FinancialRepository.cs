using HUP.Core.Entities.Financial;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class FinancialRepository : GenericRepository<StudentFee>, IFinancialRepository
    {
        public FinancialRepository(HupDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudentFee>> GetStudentFeesAsync(Guid studentId)
        {
            return await _context.StudentFees
                .Where(f => f.StudentId == studentId && !f.IsDeleted)
                .Include(f => f.Fee)
                    .ThenInclude(fee => fee.Semester)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFee>> GetStudentFeesBySemesterAsync(Guid studentId, Guid semesterId)
        {
            return await _context.StudentFees
                .Where(f => f.StudentId == studentId && f.Fee.SemesterId == semesterId && !f.IsDeleted)
                .Include(f => f.Fee)
                    .ThenInclude(fee => fee.Semester)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
