using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// GetByIdAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// GetAllAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// FindAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// FirstOrDefaultAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// AnyAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// CountAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        /// <summary>
        /// AddAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// AddRangeAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// Update methodunu çalıştırır.
        /// </summary>
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// UpdateRange methodunu çalıştırır.
        /// </summary>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        /// <summary>
        /// Delete methodunu çalıştırır.
        /// </summary>
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// DeleteRange methodunu çalıştırır.
        /// </summary>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// SaveChangesAsync methodunu çalıştırır.
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
