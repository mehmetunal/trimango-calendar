using System.Globalization;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrimangoCalendar.Core.DTOs;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Data.Context;

namespace TrimangoCalendar.Infrastructure.Services;

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
    
    /// <summary>
    /// UpdateFromTCMBAsync methodunu çalıştırır.
    /// </summary>
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
            if (dateNode?.Attributes?["Date"] is null)
            {
                _logger.LogError("TCMB XML'de date bilgisi bulunamadı");
                return;
            }
            
            var date = DateTime.ParseExact(
                dateNode.Attributes["Date"]!.Value, 
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
                var currencyCode = node.Attributes?["CurrencyCode"]?.Value;
                if (string.IsNullOrEmpty(currencyCode))
                    continue;
                
                // Sadece sistemde tanımlı para birimlerini al
                if (!currencies.Any(c => c.Code == currencyCode))
                    continue;
                
                var forexBuyingNode = node.SelectSingleNode("ForexBuying");
                var forexSellingNode = node.SelectSingleNode("ForexSelling");
                
                if (forexBuyingNode?.InnerText is null || forexSellingNode?.InnerText is null)
                    continue;
                
                var forexBuying = decimal.Parse(
                    forexBuyingNode.InnerText.Replace(".", ","));
                var forexSelling = decimal.Parse(
                    forexSellingNode.InnerText.Replace(".", ","));
                
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

    /// <summary>
    /// GetRateAsync methodunu çalıştırır.
    /// </summary>
    public async Task<ExchangeRateDto?> GetRateAsync(string baseCurrency, string targetCurrency, DateTime date)
    {
        var normalizedDate = date.Date;
        var rate = await _context.ExchangeRates
            .AsNoTracking()
            .Where(r =>
                r.BaseCurrencyCode == baseCurrency &&
                r.TargetCurrencyCode == targetCurrency &&
                r.Date.Date == normalizedDate)
            .OrderByDescending(r => r.UpdatedAt)
            .FirstOrDefaultAsync();

        if (rate is null)
        {
            return null;
        }

        return new ExchangeRateDto
        {
            BaseCurrency = rate.BaseCurrencyCode,
            TargetCurrency = rate.TargetCurrencyCode,
            Rate = rate.Rate,
            Date = rate.Date
        };
    }

    /// <summary>
    /// GetRatesForDateAsync methodunu çalıştırır.
    /// </summary>
    public async Task<List<ExchangeRateDto>> GetRatesForDateAsync(DateTime date)
    {
        var normalizedDate = date.Date;
        return await _context.ExchangeRates
            .AsNoTracking()
            .Where(r => r.Date.Date == normalizedDate)
            .OrderBy(r => r.BaseCurrencyCode)
            .ThenBy(r => r.TargetCurrencyCode)
            .Select(r => new ExchangeRateDto
            {
                BaseCurrency = r.BaseCurrencyCode,
                TargetCurrency = r.TargetCurrencyCode,
                Rate = r.Rate,
                Date = r.Date
            })
            .ToListAsync();
    }

    /// <summary>
    /// UpdateFromApiAsync methodunu çalıştırır.
    /// </summary>
    public async Task UpdateFromApiAsync()
    {
        await UpdateFromTCMBAsync();
    }

    /// <summary>
    /// SetManualRateAsync methodunu çalıştırır.
    /// </summary>
    public async Task SetManualRateAsync(string baseCurrency, string targetCurrency, decimal rate, DateTime date)
    {
        var normalizedDate = date.Date;
        var existing = await _context.ExchangeRates.FirstOrDefaultAsync(r =>
            r.BaseCurrencyCode == baseCurrency &&
            r.TargetCurrencyCode == targetCurrency &&
            r.Date.Date == normalizedDate &&
            r.Source == "Manual");

        if (existing is null)
        {
            _context.ExchangeRates.Add(new ExchangeRate
            {
                BaseCurrencyCode = baseCurrency,
                TargetCurrencyCode = targetCurrency,
                Rate = rate,
                BuyRate = rate,
                SellRate = rate,
                Date = normalizedDate,
                Source = "Manual",
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Rate = rate;
            existing.BuyRate = rate;
            existing.SellRate = rate;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
