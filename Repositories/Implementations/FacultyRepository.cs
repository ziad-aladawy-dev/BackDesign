using HUP.Core.Entities.Academics;
using HUP.Core.Interfaces;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class FacultyRepository : GenericRepository<Faculty>, IFacultyRepository
    {
        private readonly ICacheService _cacheService;
        private const string CacheKey = "faculties:list";

        public FacultyRepository(HupDbContext context, ICacheService cacheService) : base(context)
        {
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Faculty>> GetAllWithDetailsAsync()
        {
            var cached = await _cacheService.GetAsync<IEnumerable<Faculty>>(CacheKey);
            if (cached != null)
            {
                return cached;
            }

            var faculties = await _context.Faculties.Where(f =>f.IsDeleted == false).AsNoTracking().ToListAsync();
            await _cacheService.SetAsync(CacheKey, faculties, 60); // 60 minutes
            return faculties;
        }

        public override async Task AddAsync(Faculty entity)
        {
            await base.AddAsync(entity);
            await _cacheService.RemoveAsync(CacheKey);
        }

        public override async Task RemoveAsync(Guid id)
        {
            await base.RemoveAsync(id);
            await _cacheService.RemoveAsync(CacheKey);
        }

        public void Update(Faculty entity)
        {
            _context.Faculties.Update(entity);
        }

        public void SoftDelete(Guid id)
        {
            var faculty = _context.Faculties.Find(id);
            if (faculty != null)
            {
                faculty.IsDeleted = true;
                _context.Faculties.Update(faculty);
            }
        }
    }
}
