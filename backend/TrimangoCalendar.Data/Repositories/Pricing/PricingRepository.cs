using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Pricing
{
    public class PricingRepository : BaseRepository<SeasonRate>, IPricingRepository
    {
        private readonly AppDbContext _context;

        public PricingRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// GetByUnitIdAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<SeasonRate>> GetByUnitIdAsync(Guid unitId)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        /// <summary>
        /// GetActiveRateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate <= date
                    && s.EndDate >= date);
        }

        /// <summary>
        /// GetOverlappingRatesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<SeasonRate>> GetOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate < endDate
                    && s.EndDate > startDate)
                .ToListAsync();
        }

        /// <summary>
        /// GetExchangeRateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
            var rate = await _context.ExchangeRates
                .FirstOrDefaultAsync(r => r.BaseCurrencyCode == baseCurrency
                    && r.TargetCurrencyCode == targetCurrency
                    && r.Date == date);

            if (rate != null) return rate.Rate;

            // En yakın tarihli kuru bul
            var nearestRate = await _context.ExchangeRates
                .Where(r => r.BaseCurrencyCode == baseCurrency
                    && r.TargetCurrencyCode == targetCurrency)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();

            return nearestRate?.Rate ?? 1;
        }

        /// <summary>
        /// GetExchangeRatesForDateAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date)
        {
            return await _context.ExchangeRates
                .Where(r => r.Date == date)
                .ToListAsync();
        }

        /// <summary>
        /// GetActiveCurrenciesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        /// <summary>
        /// UpdateExchangeRatesAsync methodunu çalıştırır.
        /// </summary>
        public async Task UpdateExchangeRatesAsync(IEnumerable<ExchangeRate> rates)
        {
            await _context.ExchangeRates.AddRangeAsync(rates);
            await _context.SaveChangesAsync();
        }
    }
}
