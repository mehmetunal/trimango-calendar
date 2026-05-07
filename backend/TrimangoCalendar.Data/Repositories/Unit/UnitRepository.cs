using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Unit
{
    public class UnitRepository : BaseRepository<Core.Entities.Unit>, IUnitRepository
    {
        public UnitRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Unit>> GetByPropertyIdAsync(Guid propertyId)
        {
            return await _dbSet
                .Where(u => u.PropertyId == propertyId)
                .OrderBy(u => u.Floor)
                .ThenBy(u => u.UnitNumber)
                .ToListAsync();
        }

        public async Task<Core.Entities.Unit> GetByUnitNumberAsync(Guid propertyId, string unitNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.PropertyId == propertyId && u.UnitNumber == unitNumber);
        }

        public async Task<IEnumerable<Core.Entities.Unit>> GetActiveUnitsAsync(Guid propertyId)
        {
            return await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .ToListAsync();
        }

        public async Task<decimal> GetMinPriceAsync(Guid propertyId)
        {
            var minPrice = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .MinAsync(u => (decimal?)u.BasePrice);
            return minPrice ?? 0;
        }

        public async Task<decimal> GetMaxPriceAsync(Guid propertyId)
        {
            var maxPrice = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .MaxAsync(u => (decimal?)u.BasePrice);
            return maxPrice ?? 0;
        }

        public async Task<int> GetTotalCapacityAsync(Guid propertyId)
        {
            var units = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .ToListAsync();
            return units.Sum(u => u.MaxAdults + u.MaxChildren);
        }

        public async Task<bool> IsUnitNumberExistsAsync(Guid propertyId, string unitNumber, Guid? excludeUnitId = null)
        {
            var query = _dbSet.Where(u => u.PropertyId == propertyId && u.UnitNumber == unitNumber);
            if (excludeUnitId.HasValue)
                query = query.Where(u => u.Id != excludeUnitId.Value);
            return await query.AnyAsync();
        }
    }
}
