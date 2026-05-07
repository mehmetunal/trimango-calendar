public class TCMBExchangeRateService : IExchangeRateService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TCMBExchangeRateService> _logger;
    
    public TCMBExchangeRateService(
        AppDbContext context,
        HttpClient httpClient,
        ILogger<TCMBExchangeRateService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task UpdateFromTCMBAsync()
    {
        try
        {
            // TCMB XML servisi
            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            var response = await _httpClient.GetStringAsync(url);
            
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);
            
            var dateNode = xmlDoc.SelectSingleNode("//Tarih_Date");
            var date = DateTime.ParseExact(
                dateNode.Attributes["Date"].Value, 
                "MM/dd/yyyy", 
                CultureInfo.InvariantCulture);
            
            // Sadece bugünün kurları yoksa ekle
            var existingRates = await _context.ExchangeRates
                .AnyAsync(r => r.Date == date && r.Source == "TCMB");
                
            if (existingRates)
                return;
            
            var currencies = await _context.Currencies.ToListAsync();
            var rates = new List<ExchangeRate>();
            
            // TCMB'den gelen kurları işle
            var currencyNodes = xmlDoc.SelectNodes("//Currency");
            foreach (XmlNode node in currencyNodes)
            {
                var currencyCode = node.Attributes["CurrencyCode"].Value;
                
                // Sadece sistemde tanımlı para birimlerini al
                if (!currencies.Any(c => c.Code == currencyCode))
                    continue;
                
                var forexBuying = decimal.Parse(
                    node.SelectSingleNode("ForexBuying").InnerText.Replace(".", ","));
                var forexSelling = decimal.Parse(
                    node.SelectSingleNode("ForexSelling").InnerText.Replace(".", ","));
                
                rates.Add(new ExchangeRate
                {
                    BaseCurrencyCode = currencyCode,
                    TargetCurrencyCode = "TRY",
                    Rate = forexSelling,
                    BuyRate = forexBuying,
                    SellRate = forexSelling,
                    Date = date,
                    Source = "TCMB",
                    UpdatedAt = DateTime.UtcNow
                });
                
                // Ters kur (TRY -> Currency)
                rates.Add(new ExchangeRate
                {
                    BaseCurrencyCode = "TRY",
                    TargetCurrencyCode = currencyCode,
                    Rate = 1 / forexSelling,
                    BuyRate = 1 / forexSelling,
                    SellRate = 1 / forexBuying,
                    Date = date,
                    Source = "TCMB",
                    UpdatedAt = DateTime.UtcNow
                });
            }
            
            await _context.ExchangeRates.AddRangeAsync(rates);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"TCMB kurları güncellendi: {date:dd.MM.yyyy} - {rates.Count} kur eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TCMB kur güncelleme hatası");
            throw;
        }
    }
}
