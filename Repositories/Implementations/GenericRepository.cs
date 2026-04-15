using HUP.Core.Entities.Shared;
using HUP.Data;
using HUP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HUP.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly HupDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(HupDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T> GetByIdReadOnly(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<T> GetByIdTracking(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task RemoveAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                if (entity is BaseEntity softDeletable)
                {
                    softDeletable.IsDeleted = true;
                    _context.Entry(entity).State = EntityState.Modified;
                }
                else
                {
                    _dbSet.Remove(entity);
                }
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
