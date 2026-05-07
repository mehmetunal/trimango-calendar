public interface IBookingEngineService
{
    // Widget konfigürasyonu
    Task<BookingWidgetDto> GetWidgetConfigAsync(string widgetKey);
    Task<BookingWidgetDto> CreateWidgetAsync(Guid propertyId, CreateWidgetDto dto);
    Task<BookingWidgetDto> UpdateWidgetAsync(Guid widgetId, UpdateWidgetDto dto);
    
    // Arama ve rezervasyon
    Task<PropertySearchResultDto> SearchPropertyAsync(string widgetKey, BookingSearchDto search);
    Task<PropertyDetailDto> GetPropertyForBookingAsync(string widgetKey);
    Task<AvailabilityResultDto> CheckAvailabilityAsync(string widgetKey, AvailabilitySearchDto search);
    
    // Misafir işlemleri
    Task<ReservationDto> CreateBookingAsync(string widgetKey, CreateBookingDto dto);
    Task<ReservationDto> GetBookingAsync(string widgetKey, string reservationNumber, string email);
    Task<bool> CancelBookingAsync(string widgetKey, string reservationNumber, string email, string reason);
    
    // Widget embed kodu
    Task<string> GetWidgetEmbedCode(string widgetKey);
    Task<string> GetWidgetScript(string widgetKey);
}

