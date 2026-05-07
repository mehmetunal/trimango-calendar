using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Guest
{
    public interface IGuestRepository : IBaseRepository<Core.Entities.Guest>
    {
        Task<Core.Entities.Guest> GetByEmailAsync(Guid tenantId, string email);
        Task<Core.Entities.Guest> GetByPhoneAsync(Guid tenantId, string phone);
        Task<IEnumerable<Core.Entities.Guest>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Core.Entities.Guest>> SearchAsync(Guid tenantId, string searchTerm);
        Task<Core.Entities.Guest> GetWithReservationsAsync(Guid guestId);
        Task<int> GetTotalGuestsByTenantAsync(Guid tenantId);
        Task<int> GetReturningGuestsCountAsync(Guid tenantId);
    }
}
