public static async Task SeedCurrencies(AppDbContext context)
{
    if (!await context.Currencies.AnyAsync())
    {
        var currencies = new List<Currency>
        {
            new()
            {
                Code = "TRY",
                Symbol = "₺",
                Name = "Türk Lirası",
                DecimalPlaces = 2,
                CultureCode = "tr-TR",
                IsBaseCurrency = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "USD",
                Symbol = "$",
                Name = "Amerikan Doları",
                DecimalPlaces = 2,
                CultureCode = "en-US",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "EUR",
                Symbol = "€",
                Name = "Euro",
                DecimalPlaces = 2,
                CultureCode = "de-DE",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "GBP",
                Symbol = "£",
                Name = "İngiliz Sterlini",
                DecimalPlaces = 2,
                CultureCode = "en-GB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Currencies.AddRange(currencies);
        await context.SaveChangesAsync();
    }
    
    // Örnek kurlar (manuel)
    if (!await context.ExchangeRates.AnyAsync())
    {
        var today = DateTime.Today;
        var rates = new List<ExchangeRate>
        {
            // USD -> TRY
            new()
            {
                BaseCurrencyCode = "USD", TargetCurrencyCode = "TRY",
                Rate = 30.50m, BuyRate = 30.45m, SellRate = 30.55m,
                Date = today, Source = "Manual", UpdatedAt = DateTime.UtcNow
            },
            // TRY -> USD
            new()
            {
                BaseCurrencyCode = "TRY", TargetCurrencyCode = "USD",
                Rate = 1/30.50m, BuyRate = 1/30.55m, SellRate = 1/30.45m,
                Date = today, Source = "Manual", UpdatedAt = DateTime.UtcNow
            },
            // EUR -> TRY
            new()
            {
                BaseCurrencyCode = "EUR", TargetCurrencyCode = "TRY",
                Rate = 33.20m, BuyRate = 33.15m, SellRate = 33.25m,
                Date = today, Source = "Manual", UpdatedAt = DateTime.UtcNow
            },
            // TRY -> EUR
            new()
            {
                BaseCurrencyCode = "TRY", TargetCurrencyCode = "EUR",
                Rate = 1/33.20m, BuyRate = 1/33.25m, SellRate = 1/33.15m,
                Date = today, Source = "Manual", UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.ExchangeRates.AddRange(rates);
        await context.SaveChangesAsync();
    }
}
Fiyatlandırma modülü tamamlandı! Bu modül şunları içeriyor:

✅ Çoklu para birimi desteği (TRY, USD, EUR, GBP)

✅ TCMB döviz kuru entegrasyonu

✅ Sezonluk fiyatlandırma

✅ Hafta sonu/özel gün fiyatları

✅ Promosyon kodu sistemi

✅ Vergi hesaplama (KDV + Konaklama Vergisi)

✅ Ekstra yatak ücreti

✅ Money Value Object ile tip güvenli para yönetimi

Şimdi sırada MODÜL 4: REZERVASYON SİSTEMİ var. Bu modül, müsaitlik kontrolü, rezervasyon oluşturma ve yönetme işlemlerini içerecek. Devam edelim mi?

devam edelim
Harika! Rezervasyon sistemi, projenin kalbi diyebiliriz. Bu modülü çok detaylı hazırlayacağım.

📦 MODÜL 4: REZERVASYON SİSTEMİ
