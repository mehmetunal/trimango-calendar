using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Tenant
{
    public class TenantRepository : BaseRepository<Core.Entities.Tenant>, ITenantRepository
    {
        public TenantRepository(AppDbContext context) : base(context) { }

        public async Task<Core.Entities.Tenant> GetBySubdomainAsync(string subdomain)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
        }

        public async Task<Core.Entities.Tenant> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Email == email && t.IsActive);
        }

        /// <summary>
        /// IsSubdomainAvailableAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
        {
            return !await _dbSet.AnyAsync(t => t.Subdomain == subdomain);
        }

        /// <summary>
        /// IsEmailAvailableAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _dbSet.AnyAsync(t => t.Email == email);
        }

        public async Task<Core.Entities.Tenant> GetWithPropertiesAsync(Guid tenantId)
        {
            return await _dbSet
                .Include(t => t.Properties)
                .FirstOrDefaultAsync(t => t.Id == tenantId);
        }
    }
}
