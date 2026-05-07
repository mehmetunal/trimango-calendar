public class ExchangeRateUpdateJob
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<ExchangeRateUpdateJob> _logger;
    
    public ExchangeRateUpdateJob(
        IExchangeRateService exchangeRateService,
        ILogger<ExchangeRateUpdateJob> logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3)]
    public async Task Execute()
    {
        _logger.LogInformation("Döviz kuru güncelleme job'ı başladı: {Time}", DateTime.UtcNow);
        
        try
        {
            await _exchangeRateService.UpdateFromTCMBAsync();
            _logger.LogInformation("Döviz kurları başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Döviz kuru güncelleme hatası");
            throw;
        }
    }
}

