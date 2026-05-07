using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Unit
{
    public interface IUnitRepository : IBaseRepository<Core.Entities.Unit>
    {
        Task<IEnumerable<Core.Entities.Unit>> GetByPropertyIdAsync(Guid propertyId);
        Task<Core.Entities.Unit> GetByUnitNumberAsync(Guid propertyId, string unitNumber);
        Task<IEnumerable<Core.Entities.Unit>> GetActiveUnitsAsync(Guid propertyId);
        Task<decimal> GetMinPriceAsync(Guid propertyId);
        Task<decimal> GetMaxPriceAsync(Guid propertyId);
        Task<int> GetTotalCapacityAsync(Guid propertyId);
        Task<bool> IsUnitNumberExistsAsync(Guid propertyId, string unitNumber, Guid? excludeUnitId = null);
    }
}
