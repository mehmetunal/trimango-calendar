namespace TrimangoCalendar.Core.Interfaces;

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(Guid tenantId, CreateReservationDto dto);
    Task<ReservationDto> CreateAgencyReservationAsync(Guid agencyId, CreateAgencyReservationDto dto);
    Task<ReservationDto> GetByIdAsync(Guid id);
    Task<ReservationDto> GetByNumberAsync(string reservationNumber);
    Task<PaginatedResult<ReservationDto>> GetByTenantAsync(Guid tenantId, ReservationFilterDto filter);
    Task<PaginatedResult<ReservationDto>> GetByPropertyAsync(Guid propertyId, ReservationFilterDto filter);
    Task<List<ReservationDto>> GetUpcomingCheckInsAsync(Guid tenantId, DateTime date);
    Task<List<ReservationDto>> GetUpcomingCheckOutsAsync(Guid tenantId, DateTime date);
    Task<ReservationDto> UpdateStatusAsync(UpdateReservationStatusDto dto);
    Task<ReservationDto> CheckInAsync(Guid reservationId);
    Task<ReservationDto> CheckOutAsync(Guid reservationId, bool isLate = false);
    Task<bool> CancelAsync(Guid reservationId, string reason);
    Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null);
    Task<List<UnitAvailabilityDto>> GetAvailabilityAsync(Guid propertyId, DateTime startDate, DateTime endDate);
    Task<ReservationStatsDto> GetStatsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
}
