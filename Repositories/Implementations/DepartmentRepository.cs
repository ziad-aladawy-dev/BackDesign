using HUP.Core.Entities.Academics;
using HUP.Core.Interfaces;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly ICacheService _cacheService;
        private const string CacheKey = "departments:list";

        public DepartmentRepository(HupDbContext context, ICacheService cacheService) : base(context)
        {
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Department>> GetAllWithDetailsAsync()
        {
            var cached = await _cacheService.GetAsync<IEnumerable<Department>>(CacheKey);
            if (cached != null)
            {
                return cached;
            }

            var departments = await _context.Departments
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
            await _cacheService.SetAsync(CacheKey, departments, 60); // 60 minutes
            return departments;
        }

        public override async Task AddAsync(Department entity)
        {
            await base.AddAsync(entity);
            await _cacheService.RemoveAsync(CacheKey);
        }

        public override async Task RemoveAsync(Guid id)
        {
            await base.RemoveAsync(id);
            await _cacheService.RemoveAsync(CacheKey);
        }

        public async Task<Department> GetByIdWithDetailsAsync(Guid id)
        {
            var dept = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            return dept;
        }
        public async Task<Department> GetByIdTrackingAsync(Guid id)
        {
            var dept = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            return dept;
        }

        public async Task<IEnumerable<Department>> GetByFacultyIdAsync(Guid facultyId)
        {
            return await _context.DepartmentFaculties
                .Where(df => df.FacultyId == facultyId && !df.Department.IsDeleted)
                .Select(df => df.Department)
                .Where(d => !d.IsDeleted)
                .ToListAsync();
            //return await _context.Departments.Where(d => d.FacultyId == facultyId).ToListAsync();
        }
    }
}
