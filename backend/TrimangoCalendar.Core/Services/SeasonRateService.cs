// backend/TrimangoCalendar.Core/Services/SeasonRateService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrimangoCalendar.Core.DTOs;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Data.Context;

namespace TrimangoCalendar.Core.Services
{
    public class SeasonRateService : ISeasonRateService
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<SeasonRateService> _logger;

        public SeasonRateService(
            AppDbContext context,
            ICurrencyService currencyService,
            ILogger<SeasonRateService> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        #region Temel CRUD

        /// <summary>
        /// GetByIdAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRateDto> GetByIdAsync(Guid id)
        {
            var seasonRate = await _context.SeasonRates
                .Include(s => s.Unit)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seasonRate == null)
                throw new NotFoundException($"Sezon fiyatı bulunamadı: {id}");

            return MapToDto(seasonRate);
        }

        /// <summary>
        /// GetByUnitIdAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<SeasonRateDto>> GetByUnitIdAsync(Guid unitId)
        {
            var seasonRates = await _context.SeasonRates
                .Where(s => s.UnitId == unitId)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return seasonRates.Select(MapToDto).ToList();
        }

        /// <summary>
        /// CreateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRateDto> CreateAsync(CreateSeasonRateDto dto)
        {
            // Validasyon
            if (dto.StartDate >= dto.EndDate)
                throw new BusinessException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

            if (dto.WeekdayPrice <= 0)
                throw new BusinessException("Fiyat 0'dan büyük olmalıdır");

            // Çakışan sezon var mı?
            var hasOverlap = await HasOverlappingRatesAsync(dto.UnitId, dto.StartDate, dto.EndDate);
            if (hasOverlap)
                throw new BusinessException("Bu tarih aralığında başka bir sezon tanımlı. Lütfen çakışmayan bir tarih seçin.");

            // Birim var mı?
            var unit = await _context.Units.FindAsync(dto.UnitId);
            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            var seasonRate = new SeasonRate
            {
                Id = Guid.NewGuid(),
                UnitId = dto.UnitId,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                WeekdayPrice = dto.WeekdayPrice,
                WeekendPrice = dto.WeekendPrice,
                SpecialDayPrice = dto.SpecialDayPrice,
                CurrencyCode = dto.CurrencyCode ?? "TRY",
                MinStayDays = dto.MinStayDays > 0 ? dto.MinStayDays : 1,
                MaxStayDays = dto.MaxStayDays > 0 ? dto.MaxStayDays : 30,
                CancellationPolicy = dto.CancellationPolicy ?? "Flexible",
                FreeCancellationDays = dto.FreeCancellationDays,
                CancellationFee = dto.CancellationFee,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SeasonRates.Add(seasonRate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni sezon oluşturuldu: {SeasonName} - {UnitId}", dto.Name, dto.UnitId);

            return MapToDto(seasonRate);
        }

        /// <summary>
        /// UpdateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRateDto> UpdateAsync(Guid id, UpdateSeasonRateDto dto)
        {
            var seasonRate = await _context.SeasonRates.FindAsync(id);
            if (seasonRate == null)
                throw new NotFoundException($"Sezon fiyatı bulunamadı: {id}");

            // Çakışma kontrolü
            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
            {
                var hasOverlap = await HasOverlappingRatesAsync(
                    seasonRate.UnitId,
                    dto.StartDate.Value,
                    dto.EndDate.Value,
                    id);
                if (hasOverlap)
                    throw new BusinessException("Bu tarih aralığında başka bir sezon tanımlı");
            }

            // Alanları güncelle
            if (dto.Name != null) seasonRate.Name = dto.Name;
            if (dto.StartDate.HasValue) seasonRate.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) seasonRate.EndDate = dto.EndDate.Value;
            if (dto.WeekdayPrice.HasValue) seasonRate.WeekdayPrice = dto.WeekdayPrice.Value;
            if (dto.WeekendPrice.HasValue) seasonRate.WeekendPrice = dto.WeekendPrice.Value;
            if (dto.SpecialDayPrice.HasValue) seasonRate.SpecialDayPrice = dto.SpecialDayPrice.Value;
            if (dto.CurrencyCode != null) seasonRate.CurrencyCode = dto.CurrencyCode;
            if (dto.MinStayDays.HasValue) seasonRate.MinStayDays = dto.MinStayDays.Value;
            if (dto.MaxStayDays.HasValue) seasonRate.MaxStayDays = dto.MaxStayDays.Value;
            if (dto.CancellationPolicy != null) seasonRate.CancellationPolicy = dto.CancellationPolicy;
            if (dto.FreeCancellationDays.HasValue) seasonRate.FreeCancellationDays = dto.FreeCancellationDays.Value;
            if (dto.CancellationFee.HasValue) seasonRate.CancellationFee = dto.CancellationFee.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sezon güncellendi: {SeasonId} - {SeasonName}", id, seasonRate.Name);

            return MapToDto(seasonRate);
        }

        /// <summary>
        /// DeleteAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var seasonRate = await _context.SeasonRates.FindAsync(id);
            if (seasonRate == null)
                throw new NotFoundException($"Sezon fiyatı bulunamadı: {id}");

            _context.SeasonRates.Remove(seasonRate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sezon silindi: {SeasonId}", id);

            return true;
        }

        /// <summary>
        /// ToggleActiveAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> ToggleActiveAsync(Guid id)
        {
            var seasonRate = await _context.SeasonRates.FindAsync(id);
            if (seasonRate == null)
                throw new NotFoundException($"Sezon fiyatı bulunamadı: {id}");

            seasonRate.IsActive = !seasonRate.IsActive;
            await _context.SaveChangesAsync();

            return seasonRate.IsActive;
        }

        #endregion

        #region Toplu İşlemler

        /// <summary>
        /// CreateBulkAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<SeasonRateDto>> CreateBulkAsync(Guid unitId, List<CreateSeasonRateDto> dtos)
        {
            var results = new List<SeasonRateDto>();

            foreach (var dto in dtos)
            {
                dto.UnitId = unitId;
                var result = await CreateAsync(dto);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// DeleteByUnitIdAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> DeleteByUnitIdAsync(Guid unitId)
        {
            var seasonRates = await _context.SeasonRates
                .Where(s => s.UnitId == unitId)
                .ToListAsync();

            _context.SeasonRates.RemoveRange(seasonRates);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Fiyat Hesaplama

        /// <summary>
        /// GetActiveRateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date)
        {
            // Önce sezon fiyatını kontrol et
            var seasonRate = await _context.SeasonRates
                .Where(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate <= date
                    && s.EndDate >= date)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (seasonRate != null)
                return seasonRate;

            // Sezon yoksa varsayılan sezonu döndür
            return await GetOrCreateDefaultSeasonAsync(unitId);
        }

        /// <summary>
        /// GetDailyPriceAsync methodunu çalıştırır.
        /// </summary>
        public async Task<decimal> GetDailyPriceAsync(Guid unitId, DateTime date, string currencyCode)
        {
            var seasonRate = await GetActiveRateAsync(unitId, date);
            var unit = await _context.Units.FindAsync(unitId);

            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            // Temel fiyat
            decimal basePrice = seasonRate?.WeekdayPrice ?? unit.BasePrice;

            // Hafta sonu kontrolü
            bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            if (isWeekend && seasonRate?.WeekendPrice.HasValue == true)
            {
                basePrice = seasonRate.WeekendPrice.Value;
            }

            // Özel gün kontrolü
            if (IsSpecialDay(date) && seasonRate?.SpecialDayPrice.HasValue == true)
            {
                basePrice = seasonRate.SpecialDayPrice.Value;
            }

            // Özel gün fiyatı var mı?
            var specialDayRate = await _context.SpecialDayRates
                .FirstOrDefaultAsync(s => s.UnitId == unitId && s.Date.Date == date.Date);
            if (specialDayRate != null)
            {
                basePrice = specialDayRate.Price;
            }

            // Para birimi dönüşümü
            string baseCurrency = seasonRate?.CurrencyCode ?? unit.CurrencyCode;
            if (baseCurrency != currencyCode)
            {
                var rate = await _currencyService.GetExchangeRateAsync(baseCurrency, currencyCode, date);
                basePrice = Math.Round(basePrice * rate, 2);
            }

            return basePrice;
        }

        public async Task<List<DailyPriceDto>> GetDailyPricesAsync(
            Guid unitId, DateTime startDate, DateTime endDate, string currencyCode)
        {
            var prices = new List<DailyPriceDto>();
            var unit = await _context.Units.FindAsync(unitId);

            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var seasonRate = await GetActiveRateAsync(unitId, date);
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                bool isSpecial = IsSpecialDay(date);

                decimal actualPrice = await GetDailyPriceAsync(unitId, date, currencyCode);

                prices.Add(new DailyPriceDto
                {
                    Date = date,
                    DayName = date.ToString("dddd", new System.Globalization.CultureInfo("tr-TR")),
                    IsWeekend = isWeekend,
                    IsSpecialDay = isSpecial,
                    BasePrice = seasonRate?.WeekdayPrice ?? unit.BasePrice,
                    ActualPrice = actualPrice,
                    CurrencyCode = currencyCode,
                    SeasonName = seasonRate?.Name ?? "Standart Sezon"
                });
            }

            return prices;
        }

        #endregion

        #region Validasyon

        public async Task<bool> HasOverlappingRatesAsync(
            Guid unitId, DateTime startDate, DateTime endDate, Guid? excludeRateId = null)
        {
            var query = _context.SeasonRates
                .Where(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate < endDate
                    && s.EndDate > startDate);

            if (excludeRateId.HasValue)
                query = query.Where(s => s.Id != excludeRateId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// IsDateInSeasonAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> IsDateInSeasonAsync(Guid unitId, DateTime date)
        {
            return await _context.SeasonRates
                .AnyAsync(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate <= date
                    && s.EndDate >= date);
        }

        #endregion

        #region Özel Gün Fiyatları

        /// <summary>
        /// SetSpecialDayPriceAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SpecialDayRateDto> SetSpecialDayPriceAsync(SetSpecialDayPriceDto dto)
        {
            var unit = await _context.Units.FindAsync(dto.UnitId);
            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            // Varolan özel gün fiyatını kontrol et
            var existing = await _context.SpecialDayRates
                .FirstOrDefaultAsync(s => s.UnitId == dto.UnitId && s.Date.Date == dto.Date.Date);

            if (existing != null)
            {
                existing.Price = dto.Price;
                existing.CurrencyCode = dto.CurrencyCode ?? "TRY";
            }
            else
            {
                var specialDayRate = new SpecialDayRate
                {
                    Id = Guid.NewGuid(),
                    UnitId = dto.UnitId,
                    Date = dto.Date,
                    Price = dto.Price,
                    CurrencyCode = dto.CurrencyCode ?? "TRY"
                };

                _context.SpecialDayRates.Add(specialDayRate);
            }

            await _context.SaveChangesAsync();

            return new SpecialDayRateDto
            {
                Id = existing?.Id ?? Guid.NewGuid(),
                UnitId = dto.UnitId,
                Date = dto.Date,
                Price = dto.Price,
                CurrencyCode = dto.CurrencyCode ?? "TRY"
            };
        }

        public async Task<List<SpecialDayRateDto>> GetSpecialDayRatesAsync(
            Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _context.SpecialDayRates
                .Where(s => s.UnitId == unitId
                    && s.Date >= startDate
                    && s.Date <= endDate)
                .Select(s => new SpecialDayRateDto
                {
                    Id = s.Id,
                    UnitId = s.UnitId,
                    Date = s.Date,
                    Price = s.Price,
                    CurrencyCode = s.CurrencyCode
                })
                .ToListAsync();
        }

        /// <summary>
        /// DeleteSpecialDayRateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> DeleteSpecialDayRateAsync(Guid id)
        {
            var specialDayRate = await _context.SpecialDayRates.FindAsync(id);
            if (specialDayRate == null)
                throw new NotFoundException($"Özel gün fiyatı bulunamadı: {id}");

            _context.SpecialDayRates.Remove(specialDayRate);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Varsayılan Sezon

        /// <summary>
        /// GetOrCreateDefaultSeasonAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRate> GetOrCreateDefaultSeasonAsync(Guid unitId)
        {
            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            // Varsayılan sezon var mı?
            var defaultSeason = await _context.SeasonRates
                .FirstOrDefaultAsync(s => s.UnitId == unitId && s.Name == "Standart Sezon");

            if (defaultSeason != null)
                return defaultSeason;

            // Yeni varsayılan sezon oluştur
            defaultSeason = new SeasonRate
            {
                Id = Guid.NewGuid(),
                UnitId = unitId,
                Name = "Standart Sezon",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2030, 12, 31),
                WeekdayPrice = unit.BasePrice,
                CurrencyCode = unit.CurrencyCode,
                MinStayDays = 1,
                MaxStayDays = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SeasonRates.Add(defaultSeason);
            await _context.SaveChangesAsync();

            return defaultSeason;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// IsSpecialDay methodunu çalıştırır.
        /// </summary>
        private bool IsSpecialDay(DateTime date)
        {
            // Türkiye'deki resmi tatiller ve özel günler
            var specialDays = new List<(int Month, int Day)>
            {
                (1, 1),   // Yılbaşı
                (4, 23),  // Ulusal Egemenlik ve Çocuk Bayramı
                (5, 1),   // Emek ve Dayanışma Günü
                (5, 19),  // Atatürk'ü Anma, Gençlik ve Spor Bayramı
                (7, 15),  // Demokrasi ve Milli Birlik Günü
                (8, 30),  // Zafer Bayramı
                (10, 29), // Cumhuriyet Bayramı
            };

            return specialDays.Any(sd => sd.Month == date.Month && sd.Day == date.Day);
        }

        /// <summary>
        /// MapToDto methodunu çalıştırır.
        /// </summary>
        private SeasonRateDto MapToDto(SeasonRate seasonRate)
        {
            return new SeasonRateDto
            {
                Id = seasonRate.Id,
                UnitId = seasonRate.UnitId,
                Name = seasonRate.Name,
                StartDate = seasonRate.StartDate,
                EndDate = seasonRate.EndDate,
                WeekdayPrice = seasonRate.WeekdayPrice,
                WeekendPrice = seasonRate.WeekendPrice,
                SpecialDayPrice = seasonRate.SpecialDayPrice,
                CurrencyCode = seasonRate.CurrencyCode,
                MinStayDays = seasonRate.MinStayDays,
                MaxStayDays = seasonRate.MaxStayDays,
                CancellationPolicy = seasonRate.CancellationPolicy,
                FreeCancellationDays = seasonRate.FreeCancellationDays,
                CancellationFee = seasonRate.CancellationFee,
                IsActive = seasonRate.IsActive,
                CreatedAt = seasonRate.CreatedAt
            };
        }

        #endregion
    }
}