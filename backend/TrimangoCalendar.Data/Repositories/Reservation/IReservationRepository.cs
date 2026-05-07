using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Reservation
{
    public interface IReservationRepository : IBaseRepository<Core.Entities.Reservation>
    {
        Task<IEnumerable<Core.Entities.Reservation>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<Core.Entities.Reservation?> GetByReservationNumberAsync(string reservationNumber);
        Task<Core.Entities.Reservation?> GetFullDetailAsync(Guid reservationId);
        Task<IEnumerable<Core.Entities.Reservation>> GetByDateRangeAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckInsAsync(Guid tenantId, DateTime date);
        Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckOutsAsync(Guid tenantId, DateTime date);
        Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null);
        Task<int> GetTotalCountByTenantAsync(Guid tenantId);
        Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetReservationCountByStatusAsync(Guid tenantId, ReservationStatus status);
    }
}
