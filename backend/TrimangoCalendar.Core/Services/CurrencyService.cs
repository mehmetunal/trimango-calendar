public class CurrencyService : ICurrencyService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IExchangeRateService _exchangeRateService;
    
    public CurrencyService(
        AppDbContext context,
        IMemoryCache cache,
        IExchangeRateService exchangeRateService)
    {
        _context = context;
        _cache = cache;
        _exchangeRateService = exchangeRateService;
    }
    
    public async Task<List<CurrencyDto>> GetActiveCurrenciesAsync()
    {
        return await _cache.GetOrCreateAsync("active_currencies", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            
            var currencies = await _context.Currencies
                .Where(c => c.IsActive)
                .ToListAsync();
                
            return _mapper.Map<List<CurrencyDto>>(currencies);
        });
    }
    
    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        
        // Aynı para birimi ise 1 döndür
        if (fromCurrency == toCurrency)
            return 1;
        
        string cacheKey = $"rate_{fromCurrency}_{toCurrency}_{targetDate:yyyyMMdd}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            
            // Önce direkt kur var mı kontrol et
            var rate = await _context.ExchangeRates
                .FirstOrDefaultAsync(r => 
                    r.BaseCurrencyCode == fromCurrency && 
                    r.TargetCurrencyCode == toCurrency && 
                    r.Date == targetDate);
            
            if (rate != null)
                return rate.SellRate; // Satış kurunu kullan
            
            // TRY üzerinden çapraz kur hesapla
            if (fromCurrency != "TRY" && toCurrency != "TRY")
            {
                var rateToTRY = await GetExchangeRateAsync(fromCurrency, "TRY", targetDate);
                var rateFromTRY = await GetExchangeRateAsync("TRY", toCurrency, targetDate);
                return rateToTRY * rateFromTRY;
            }
            
            // Kur bulunamazsa en yakın tarihli kuru al
            var nearest = await _context.ExchangeRates
                .Where(r => r.BaseCurrencyCode == fromCurrency && r.TargetCurrencyCode == toCurrency)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
                
            return nearest?.SellRate ?? throw new Exception($"Döviz kuru bulunamadı: {fromCurrency} -> {toCurrency}");
        });
    }
    
    public Money Convert(Money amount, string targetCurrencyCode, DateTime? date = null)
    {
        if (amount.Currency.Code == targetCurrencyCode)
            return amount;
            
        var rate = GetExchangeRateAsync(amount.Currency.Code, targetCurrencyCode, date)
            .GetAwaiter().GetResult();
            
        return amount.ConvertTo(
            _context.Currencies.Find(targetCurrencyCode), 
            rate);
    }
    
    public async Task UpdateExchangeRatesAsync()
    {
        await _exchangeRateService.UpdateFromTCMBAsync();
    }
}
