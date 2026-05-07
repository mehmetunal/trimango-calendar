using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Reservation
{
    public class ReservationRepository : BaseRepository<Core.Entities.Reservation>, IReservationRepository
    {
        public ReservationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Core.Entities.Reservation> GetByReservationNumberAsync(string reservationNumber)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.ReservationNumber == reservationNumber);
        }

        public async Task<Core.Entities.Reservation> GetFullDetailAsync(Guid reservationId)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetByDateRangeAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(r => r.UnitId == unitId
                    && r.Status != ReservationStatus.Cancelled
                    && r.Status != ReservationStatus.NoShow
                    && r.CheckIn < endDate
                    && r.CheckOut > startDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckInsAsync(Guid tenantId, DateTime date)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId
                    && r.CheckIn.Date == date.Date
                    && r.Status == ReservationStatus.Confirmed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckOutsAsync(Guid tenantId, DateTime date)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId
                    && r.CheckOut.Date == date.Date
                    && r.Status == ReservationStatus.CheckedIn)
                .ToListAsync();
        }

        /// <summary>
        /// IsUnitAvailableAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null)
        {
            var query = _dbSet.Where(r => r.UnitId == unitId
                && r.Status != ReservationStatus.Cancelled
                && r.Status != ReservationStatus.NoShow
                && r.CheckIn < checkOut
                && r.CheckOut > checkIn);

            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);

            return !await query.AnyAsync();
        }

        /// <summary>
        /// GetTotalCountByTenantAsync methodunu çalıştırır.
        /// </summary>
        public async Task<int> GetTotalCountByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(r => r.TenantId == tenantId);
        }

        /// <summary>
        /// GetTotalRevenueAsync methodunu çalıştırır.
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(r => r.TenantId == tenantId
                && r.Status != ReservationStatus.Cancelled);

            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate.Value);

            return await query.SumAsync(r => r.TotalAmount);
        }

        /// <summary>
        /// GetReservationCountByStatusAsync methodunu çalıştırır.
        /// </summary>
        public async Task<int> GetReservationCountByStatusAsync(Guid tenantId, ReservationStatus status)
        {
            return await _dbSet.CountAsync(r => r.TenantId == tenantId && r.Status == status);
        }
    }
}
