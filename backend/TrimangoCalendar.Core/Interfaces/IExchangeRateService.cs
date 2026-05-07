namespace TrimangoCalendar.Core.Interfaces;

public interface IExchangeRateService
{
    Task<ExchangeRateDto?> GetRateAsync(string baseCurrency, string targetCurrency, DateTime date);
    Task<List<ExchangeRateDto>> GetRatesForDateAsync(DateTime date);
    Task UpdateFromTCMBAsync();
    Task UpdateFromApiAsync();
    Task SetManualRateAsync(string baseCurrency, string targetCurrency, decimal rate, DateTime date);
}

