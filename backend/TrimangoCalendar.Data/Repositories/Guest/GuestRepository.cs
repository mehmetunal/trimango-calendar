using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Guest
{
    public class GuestRepository : BaseRepository<Core.Entities.Guest>, IGuestRepository
    {
        public GuestRepository(AppDbConext context) : base(context) { }

        public async Task<Core.Entities.Guest> GetByEmailAsync(Guid tenantId, string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.TenantId == tenantId && g.Email == email);
        }

        public async Task<Core.Entities.Guest> GetByPhoneAsync(Guid tenantId, string phone)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.TenantId == tenantId && g.Phone == phone);
        }

        public async Task<IEnumerable<Core.Entities.Guest>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId)
                .OrderByDescending(g => g.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Guest>> SearchAsync(Guid tenantId, string searchTerm)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId &&
                    (g.FirstName.Contains(searchTerm) ||
                     g.LastName.Contains(searchTerm) ||
                     g.Email.Contains(searchTerm) ||
                     g.Phone.Contains(searchTerm)))
                .OrderByDescending(g => g.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Core.Entities.Guest> GetWithReservationsAsync(Guid guestId)
        {
            return await _dbSet
                .Include(g => g.Reservations)
                .FirstOrDefaultAsync(g => g.Id == guestId);
        }

        public async Task<int> GetTotalGuestsByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(g => g.TenantId == tenantId);
        }

        public async Task<int> GetReturningGuestsCountAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId)
                .CountAsync(g => g.Reservations.Count > 1);
        }
    }
}
