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
        private readonly AppDbConext _context;

        public PricingRepository(AppDbConext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SeasonRate>> GetByUnitIdAsync(Guid unitId)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate <= date
                    && s.EndDate >= date);
        }

        public async Task<IEnumerable<SeasonRate>> GetOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate < endDate
                    && s.EndDate > startDate)
                .ToListAsync();
        }

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

        public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date)
        {
            return await _context.ExchangeRates
                .Where(r => r.Date == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task UpdateExchangeRatesAsync(IEnumerable<ExchangeRate> rates)
        {
            await _context.ExchangeRates.AddRangeAsync(rates);
            await _context.SaveChangesAsync();
        }
    }
}
