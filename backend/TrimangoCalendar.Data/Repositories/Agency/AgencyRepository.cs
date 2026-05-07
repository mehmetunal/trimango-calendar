using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Agency
{
    public class AgencyRepository : BaseRepository<Core.Entities.Agency>, IAgencyRepository
    {
        public AgencyRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Agency>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(a => a.IsActive)
                .OrderBy(a => a.CompanyName)
                .ToListAsync();
        }

        public async Task<Core.Entities.Agency> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<IEnumerable<Core.Entities.Agency>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Where(a => a.CompanyName.Contains(searchTerm) ||
                           a.Email.Contains(searchTerm) ||
                           a.ContactPerson.Contains(searchTerm))
                .OrderBy(a => a.CompanyName)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Core.Entities.AgencyAuthorization> GetAuthorizationAsync(Guid agencyId, Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PropertyId == propertyId && a.IsActive);
        }

        public async Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByAgencyAsync(Guid agencyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .Where(a => a.AgencyId == agencyId && a.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByPropertyAsync(Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .Where(a => a.PropertyId == propertyId && a.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// HasAuthorizationAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> HasAuthorizationAsync(Guid agencyId, Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .AnyAsync(a => a.AgencyId == agencyId && a.PropertyId == propertyId && a.IsActive);
        }

        /// <summary>
        /// UpdateAllotmentUsageAsync methodunu çalıştırır.
        /// </summary>
        public async Task UpdateAllotmentUsageAsync(Guid authorizationId, int usedAllotment)
        {
            var auth = await _context.AgencyAuthorizations.FindAsync(authorizationId);
            if (auth != null)
            {
                auth.UsedAllotment = usedAllotment;
                await _context.SaveChangesAsync();
            }
        }
    }
}
