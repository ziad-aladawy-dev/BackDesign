using HUP.Core.Entities.Academics;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations;

public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(HupDbContext context) : base(context)
    {
    }

    public async Task<Semester?> GetActiveSemesterAsync()
    {
        return await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
    }
}