using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Calendar
{
    public class CalendarRepository : BaseRepository<CalendarBlock>, ICalendarRepository
    {
        public CalendarRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// GetBlocksByUnitAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<CalendarBlock>> GetBlocksByUnitAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.UnitId == unitId
                    && b.IsActive
                    && b.StartDate < endDate
                    && b.EndDate > startDate)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// GetBlocksByPropertyAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<CalendarBlock>> GetBlocksByPropertyAsync(Guid propertyId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(b => b.Unit)
                .Where(b => b.Unit.PropertyId == propertyId
                    && b.IsActive
                    && b.StartDate < endDate
                    && b.EndDate > startDate)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// HasOverlappingBlockAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> HasOverlappingBlockAsync(Guid unitId, DateTime startDate, DateTime endDate, Guid? excludeBlockId = null)
        {
            var query = _dbSet.Where(b => b.UnitId == unitId
                && b.IsActive
                && b.StartDate < endDate
                && b.EndDate > startDate);

            if (excludeBlockId.HasValue)
                query = query.Where(b => b.Id != excludeBlockId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// GetActiveBlocksAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<CalendarBlock>> GetActiveBlocksAsync(Guid unitId)
        {
            return await _dbSet
                .Where(b => b.UnitId == unitId && b.IsActive)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// UnblockDatesAsync methodunu çalıştırır.
        /// </summary>
        public async Task UnblockDatesAsync(Guid blockId)
        {
            var block = await _dbSet.FindAsync(blockId);
            if (block != null)
            {
                block.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// UnblockAllExpiredAsync methodunu çalıştırır.
        /// </summary>
        public async Task UnblockAllExpiredAsync()
        {
            var expiredBlocks = await _dbSet
                .Where(b => b.IsActive && b.EndDate < DateTime.Today)
                .ToListAsync();

            foreach (var block in expiredBlocks)
                block.IsActive = false;

            await _context.SaveChangesAsync();
        }
    }
}
