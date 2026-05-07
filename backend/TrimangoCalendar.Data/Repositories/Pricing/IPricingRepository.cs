using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Pricing
{
    public interface IPricingRepository : IBaseRepository<SeasonRate>
    {
        Task<IEnumerable<SeasonRate>> GetByUnitIdAsync(Guid unitId);
        Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date);
        Task<IEnumerable<SeasonRate>> GetOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date);
        Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date);
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task UpdateExchangeRatesAsync(IEnumerable<ExchangeRate> rates);
    }
}
