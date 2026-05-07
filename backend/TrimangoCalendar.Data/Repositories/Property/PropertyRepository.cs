using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Property
{
    public class PropertyRepository : BaseRepository<Core.Entities.Property>, IPropertyRepository
    {
        public PropertyRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Property>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Core.Entities.Property?> GetBySlugAsync(Guid tenantId, string slug)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Slug == slug);
        }

        public async Task<Core.Entities.Property?> GetWithUnitsAsync(Guid propertyId)
        {
            return await _dbSet
                .Include(p => p.Units.Where(u => u.IsActive))
                .FirstOrDefaultAsync(p => p.Id == propertyId);
        }

        public async Task<Core.Entities.Property?> GetFullDetailAsync(Guid propertyId)
        {
            return await _dbSet
                .Include(p => p.Units)
                .Include(p => p.Tenant)
                .FirstOrDefaultAsync(p => p.Id == propertyId);
        }

        public async Task<IEnumerable<Core.Entities.Property>> SearchAsync(string city, string type, bool? isActive)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.City.Contains(city));

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<PropertyType>(type, out var propertyType))
                query = query.Where(p => p.Type == propertyType);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// GetPropertyCountByTenantAsync methodunu çalıştırır.
        /// </summary>
        public async Task<int> GetPropertyCountByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(p => p.TenantId == tenantId && p.IsActive);
        }
    }
}
