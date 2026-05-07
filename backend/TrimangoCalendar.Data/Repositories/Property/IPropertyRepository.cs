using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Property
{
    public interface IPropertyRepository : IBaseRepository<Core.Entities.Property>
    {
        Task<IEnumerable<Core.Entities.Property>> GetByTenantIdAsync(Guid tenantId);
        Task<Core.Entities.Property?> GetBySlugAsync(Guid tenantId, string slug);
        Task<Core.Entities.Property?> GetWithUnitsAsync(Guid propertyId);
        Task<Core.Entities.Property?> GetFullDetailAsync(Guid propertyId);
        Task<IEnumerable<Core.Entities.Property>> SearchAsync(string city, string type, bool? isActive);
        Task<int> GetPropertyCountByTenantAsync(Guid tenantId);
    }
}
