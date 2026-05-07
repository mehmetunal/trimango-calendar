using System;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Tenant
{
    public interface ITenantRepository : IBaseRepository<Core.Entities.Tenant>
    {
        Task<Core.Entities.Tenant?> GetBySubdomainAsync(string subdomain);
        Task<Core.Entities.Tenant?> GetByEmailAsync(string email);
        Task<bool> IsSubdomainAvailableAsync(string subdomain);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<Core.Entities.Tenant?> GetWithPropertiesAsync(Guid tenantId);
    }
}
