// backend/TrimangoCalendar.Core/Interfaces/ISeasonRateService.cs
using TrimangoCalendar.Core.DTOs;
using TrimangoCalendar.Core.Entities;

namespace TrimangoCalendar.Core.Interfaces
{
    public interface ISeasonRateService
    {
        // Temel CRUD
        Task<SeasonRateDto> GetByIdAsync(Guid id);
        Task<List<SeasonRateDto>> GetByUnitIdAsync(Guid unitId);
        Task<List<SeasonRateDto>> GetByUnitAsync(Guid unitId);
        Task<SeasonRateDto> CreateAsync(CreateSeasonRateDto dto);
        Task<SeasonRateDto> UpdateAsync(Guid id, UpdateSeasonRateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ToggleActiveAsync(Guid id);
        
        // Toplu işlemler
        Task<List<SeasonRateDto>> CreateBulkAsync(Guid unitId, List<CreateSeasonRateDto> dtos);
        Task<bool> DeleteByUnitIdAsync(Guid unitId);
        
        // Fiyat hesaplama
        Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date);
        Task<decimal> GetDailyPriceAsync(Guid unitId, DateTime date, string currencyCode);
        Task<List<DailyPriceDto>> GetDailyPricesAsync(Guid unitId, DateTime startDate, DateTime endDate, string currencyCode);
        
        // Validasyon
        Task<bool> HasOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate, Guid? excludeRateId = null);
        Task<bool> IsDateInSeasonAsync(Guid unitId, DateTime date);
        
        // Özel gün fiyatları
        Task<SpecialDayRateDto> SetSpecialDayPriceAsync(SetSpecialDayPriceDto dto);
        Task<List<SpecialDayRateDto>> GetSpecialDayRatesAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<bool> DeleteSpecialDayRateAsync(Guid id);
        
        // Varsayılan sezon
        Task<SeasonRate> GetOrCreateDefaultSeasonAsync(Guid unitId);
    }
}
