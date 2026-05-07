using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Agency
{
    public interface IAgencyRepository : IBaseRepository<Core.Entities.Agency>
    {
        Task<IEnumerable<Core.Entities.Agency>> GetAllActiveAsync();
        Task<Core.Entities.Agency> GetByEmailAsync(string email);
        Task<IEnumerable<Core.Entities.Agency>> SearchAsync(string searchTerm);
        Task<Core.Entities.AgencyAuthorization> GetAuthorizationAsync(Guid agencyId, Guid propertyId);
        Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByAgencyAsync(Guid agencyId);
        Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByPropertyAsync(Guid propertyId);
        Task<bool> HasAuthorizationAsync(Guid agencyId, Guid propertyId);
        Task UpdateAllotmentUsageAsync(Guid authorizationId, int usedAllotment);
    }
}
