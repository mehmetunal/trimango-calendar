public class PricingService : IPricingService
{
    private readonly AppDbContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly IMemoryCache _cache;
    
    public PricingService(
        AppDbContext context,
        ICurrencyService currencyService,
        IMemoryCache cache)
    {
        _context = context;
        _currencyService = currencyService;
        _cache = cache;
    }
    
    public async Task<PriceCalculationResult> CalculatePriceAsync(PriceCalculationRequest request)
    {
        // Validasyon
        if (request.CheckIn >= request.CheckOut)
            throw new BusinessException("Giriş tarihi çıkış tarihinden önce olmalıdır");
        
        if (request.CheckIn < DateTime.Today)
            throw new BusinessException("Geçmiş tarih için rezervasyon yapılamaz");
        
        var unit = await _context.Units
            .Include(u => u.SeasonRates.Where(s => s.IsActive))
            .FirstOrDefaultAsync(u => u.Id == request.UnitId);
            
        if (unit == null)
            throw new NotFoundException("Birim bulunamadı");
        
        // Kapasite kontrolü
        if (request.Adults > unit.MaxAdults)
            throw new BusinessException($"Bu birim maksimum {unit.MaxAdults} yetişkin kapasitelidir");
        
        if (request.Children > unit.MaxChildren)
            throw new BusinessException($"Bu birim maksimum {unit.MaxChildren} çocuk kapasitelidir");
        
        // Geceleme sayısı
        var totalNights = (request.CheckOut - request.CheckIn).Days;
        
        // Günlük fiyatları hesapla
        var dailyPrices = await CalculateDailyPricesAsync(unit, request.CheckIn, request.CheckOut, request.CurrencyCode);
        
        // Toplam oda fiyatı
        var totalRoomPrice = dailyPrices.Sum(d => d.ActualPrice);
        var basePrice = new Money(totalRoomPrice, await GetCurrency(request.CurrencyCode));
        
        // Ekstra yatak ücreti
        var extraBedCharge = CalculateExtraBedCharge(unit, request.Adults, request.Children, request.CurrencyCode);
        
        // Promosyon/indirim kontrolü
        Money? promotionDiscount = null;
        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            var discount = await CalculatePromotionDiscount(
                request.PromoCode, request.UnitId, totalRoomPrice, request.CurrencyCode);
            if (discount.HasValue)
                promotionDiscount = discount;
        }
        
        // Vergi hesaplama (KDV %10 - Konaklama Vergisi %2)
        var taxRate = 0.12m; // %10 KDV + %2 Konaklama Vergisi
        var taxableAmount = promotionDiscount.HasValue 
            ? basePrice.Amount - promotionDiscount.Value.Amount 
            : basePrice.Amount;
        
        if (extraBedCharge != null)
            taxableAmount += extraBedCharge.Amount;
        
        var taxAmount = new Money(taxableAmount * taxRate, basePrice.Currency);
        
        // Servis ücreti (Platform komisyonu %3)
        var serviceFee = new Money(taxableAmount * 0.03m, basePrice.Currency);
        
        // Genel toplam
        var grandTotal = new Money(
            taxableAmount + taxAmount.Amount + serviceFee.Amount,
            basePrice.Currency);
        
        // Sonuç
        var result = new PriceCalculationResult
        {
            UnitId = unit.Id,
            UnitName = unit.Name,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            TotalNights = totalNights,
            Adults = request.Adults,
            Children = request.Children,
            
            Breakdown = new PriceBreakdown
            {
                DailyPrices = dailyPrices,
                BasePrice = new PriceDto
                {
                    Amount = totalRoomPrice,
                    CurrencyCode = request.CurrencyCode,
                    FormattedPrice = $"{basePrice.Currency.Symbol}{totalRoomPrice:N2}"
                },
                ExtraBedCharge = extraBedCharge != null ? new PriceDto
                {
                    Amount = extraBedCharge.Amount,
                    CurrencyCode = request.CurrencyCode,
                    FormattedPrice = extraBedCharge.ToFormattedString()
                } : null,
                PromotionDiscount = promotionDiscount.HasValue ? new PriceDto
                {
                    Amount = promotionDiscount.Value.Amount,
                    CurrencyCode = request.CurrencyCode,
                    FormattedPrice = $"-{promotionDiscount.Value.ToFormattedString()}"
                } : null
            },
            
            TotalPrice = new PriceDto
            {
                Amount = basePrice.Amount,
                CurrencyCode = request.CurrencyCode,
                FormattedPrice = basePrice.ToFormattedString()
            },
            
            AverageNightlyPrice = new PriceDto
            {
                Amount = totalRoomPrice / totalNights,
                CurrencyCode = request.CurrencyCode,
                FormattedPrice = $"{basePrice.Currency.Symbol}{totalRoomPrice / totalNights:N2}"
            },
            
            TaxAmount = new PriceDto
            {
                Amount = taxAmount.Amount,
                CurrencyCode = request.CurrencyCode,
                FormattedPrice = taxAmount.ToFormattedString()
            },
            
            ServiceFee = new PriceDto
            {
                Amount = serviceFee.Amount,
                CurrencyCode = request.CurrencyCode,
                FormattedPrice = serviceFee.ToFormattedString()
            },
            
            GrandTotal = new PriceDto
            {
                Amount = grandTotal.Amount,
                CurrencyCode = request.CurrencyCode,
                FormattedPrice = grandTotal.ToFormattedString()
            }
        };
        
        return result;
    }
    
    private async Task<List<DailyPrice>> CalculateDailyPricesAsync(
        Unit unit, DateTime checkIn, DateTime checkOut, string targetCurrency)
    {
        var dailyPrices = new List<DailyPrice>();
        var exchangeRate = 1m;
        
        // Döviz kuru hesapla
        if (unit.CurrencyCode != targetCurrency)
        {
            exchangeRate = await _currencyService.GetExchangeRateAsync(
                unit.CurrencyCode, targetCurrency, DateTime.Today);
        }
        
        for (var date = checkIn; date < checkOut; date = date.AddDays(1))
        {
            // O güne ait sezon fiyatını bul
            var seasonRate = unit.SeasonRates?
                .FirstOrDefault(s => date >= s.StartDate && date <= s.EndDate);
            
            var basePrice = seasonRate?.WeekdayPrice ?? unit.BasePrice;
            var actualPrice = basePrice;
            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            
            // Hafta sonu fiyatı
            if (isWeekend && seasonRate?.WeekendPrice.HasValue == true)
            {
                actualPrice = seasonRate.WeekendPrice.Value;
            }
            
            // Özel gün fiyatı (öncelikli)
            if (IsSpecialDay(date) && seasonRate?.SpecialDayPrice.HasValue == true)
            {
                actualPrice = seasonRate.SpecialDayPrice.Value;
            }
            
            // Döviz dönüşümü
            if (unit.CurrencyCode != targetCurrency)
            {
                actualPrice = actualPrice * exchangeRate;
            }
            
            dailyPrices.Add(new DailyPrice
            {
                Date = date,
                DayName = date.ToString("dddd", new CultureInfo("tr-TR")),
                IsWeekend = isWeekend,
                BasePrice = basePrice,
                ActualPrice = Math.Round(actualPrice, 2),
                CurrencyCode = targetCurrency,
                SeasonName = seasonRate?.Name ?? "Standart Sezon"
            });
        }
        
        return dailyPrices;
    }
    
    private Money? CalculateExtraBedCharge(Unit unit, int adults, int children, string currencyCode)
    {
        var totalGuests = adults + children;
        var capacity = unit.MaxAdults + unit.MaxChildren;
        
        if (totalGuests <= capacity || !unit.ExtraBedPrice.HasValue)
            return null;
        
        var extraBeds = totalGuests - capacity;
        if (extraBeds <= 0 || extraBeds > unit.ExtraBedCapacity)
            return null;
        
        var charge = unit.ExtraBedPrice.Value * extraBeds;
        return new Money(charge, _context.Currencies.Find(currencyCode));
    }
    
    private bool IsSpecialDay(DateTime date)
    {
        // Türkiye'deki resmi tatiller
        var specialDays = new[]
        {
            new { Month = 1, Day = 1 },   // Yılbaşı
            new { Month = 4, Day = 23 },  // Ulusal Egemenlik
            new { Month = 5, Day = 1 },   // İşçi Bayramı
            new { Month = 5, Day = 19 },  // Atatürk'ü Anma
            new { Month = 7, Day = 15 },  // Demokrasi Bayramı
            new { Month = 8, Day = 30 },  // Zafer Bayramı
            new { Month = 10, Day = 29 }, // Cumhuriyet Bayramı
        };
        
        return specialDays.Any(sd => sd.Month == date.Month && sd.Day == date.Day);
    }
    
    private async Task<Money?> CalculatePromotionDiscount(
        string promoCode, Guid unitId, decimal totalPrice, string currencyCode)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => 
                p.Code == promoCode && 
                p.IsActive &&
                p.StartDate <= DateTime.UtcNow &&
                p.EndDate >= DateTime.UtcNow &&
                (p.UnitId == unitId || p.PropertyId != null));
        
        if (promotion == null || promotion.UsedCount >= promotion.MaxUsageCount)
            return null;
        
        decimal discountAmount;
        if (promotion.Type == PromotionType.Percentage)
        {
            discountAmount = totalPrice * (promotion.DiscountValue / 100);
        }
        else
        {
            discountAmount = promotion.DiscountValue;
            if (promotion.CurrencyCode != currencyCode)
            {
                var rate = await _currencyService.GetExchangeRateAsync(
                    promotion.CurrencyCode, currencyCode, DateTime.Today);
                discountAmount = discountAmount * rate;
            }
        }
        
        return new Money(Math.Min(discountAmount, totalPrice), await GetCurrency(currencyCode));
    }
    
    private async Task<Currency> GetCurrency(string code)
    {
        return await _context.Currencies.FindAsync(code) 
            ?? throw new NotFoundException($"Para birimi bulunamadı: {code}");
    }
}
