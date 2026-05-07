namespace TrimangoCalendar.Core.Interfaces;

public interface ICalendarService
{
    // Takvim blokajı
    Task<CalendarBlockDto> BlockDatesAsync(BlockDatesDto dto);
    Task UnblockDatesAsync(Guid blockId);
    Task<List<CalendarBlockDto>> GetBlocksAsync(Guid unitId, DateTime start, DateTime end);
    
    // Takvim fiyatları
    Task SetDailyPriceAsync(SetDailyPriceDto dto);
    Task SetBulkPricesAsync(BulkPriceDto dto);
    Task<List<CalendarPriceDto>> GetCalendarPricesAsync(Guid unitId, DateTime start, DateTime end);
    
    // Acente takvimi
    Task<AgencyCalendarDto> GetAgencyCalendarAsync(Guid agencyId, Guid propertyId, DateTime start, DateTime end);
    Task<bool> CanAgencyBookAsync(Guid agencyId, Guid unitId, DateTime checkIn, DateTime checkOut);
    
    // Mülk sahibi takvimi (tüm acenteleri görür)
    Task<OwnerCalendarDto> GetOwnerCalendarAsync(Guid propertyId, DateTime start, DateTime end);
}

