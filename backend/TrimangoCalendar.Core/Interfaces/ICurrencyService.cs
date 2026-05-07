namespace TrimangoCalendar.Core.Interfaces;

public interface ICurrencyService
{
    Task<List<CurrencyDto>> GetActiveCurrenciesAsync();
    Task<CurrencyDto> GetByCodeAsync(string code);
    Task<bool> AddCurrencyAsync(CurrencyDto dto);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? date = null);
    Task UpdateExchangeRatesAsync();
    Money Convert(Money amount, string targetCurrencyCode, DateTime? date = null);
}

