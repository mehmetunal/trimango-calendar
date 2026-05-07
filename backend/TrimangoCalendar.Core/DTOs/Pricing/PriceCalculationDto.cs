public class PriceCalculationRequest
{
    [Required]
    public Guid UnitId { get; set; }
    
    [Required]
    public DateTime CheckIn { get; set; }
    
    [Required]
    public DateTime CheckOut { get; set; }
    
    [Required]
    [Range(1, 20)]
    public int Adults { get; set; } = 1;
    
    [Range(0, 10)]
    public int Children { get; set; } = 0;
    
    public string CurrencyCode { get; set; } = "TRY";
    public string PromoCode { get; set; }
}

public class PriceCalculationResult
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int TotalNights { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    
    // Fiyat Kırılımı
    public PriceBreakdown Breakdown { get; set; }
    
    // Toplam Fiyat
    public PriceDto TotalPrice { get; set; }
    public PriceDto AverageNightlyPrice { get; set; }
    
    // Vergiler ve Harçlar
    public PriceDto TaxAmount { get; set; }
    public PriceDto ServiceFee { get; set; }
    public PriceDto GrandTotal { get; set; }
    
    // İptal Politikası
    public CancellationPolicyDto CancellationPolicy { get; set; }
}

public class PriceBreakdown
{
    public List<DailyPrice> DailyPrices { get; set; } = new();
    public PriceDto BasePrice { get; set; }
    public PriceDto? WeekendSurcharge { get; set; }
    public PriceDto? SeasonSurcharge { get; set; }
    public PriceDto? ExtraBedCharge { get; set; }
    public PriceDto? PromotionDiscount { get; set; }
}

public class DailyPrice
{
    public DateTime Date { get; set; }
    public string DayName { get; set; }
    public bool IsWeekend { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ActualPrice { get; set; }
    public string CurrencyCode { get; set; }
    public string SeasonName { get; set; }
}

public class CancellationPolicyDto
{
    public string PolicyType { get; set; }
    public int FreeCancellationDays { get; set; }
    public decimal? CancellationFee { get; set; }
    public string Description { get; set; }
}

