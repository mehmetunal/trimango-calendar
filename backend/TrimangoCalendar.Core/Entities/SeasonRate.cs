namespace TrimangoCalendar.Core.Entities;

public class SeasonRate
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Name { get; set; } // "Yaz Sezonu 2024", "Bayram Tatili"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Hafta içi fiyatları
    public decimal WeekdayPrice { get; set; }
    
    // Hafta sonu fiyatları (Cumartesi, Pazar)
    public decimal? WeekendPrice { get; set; }
    
    // Özel gün fiyatları (Bayram, Yılbaşı vb.)
    public decimal? SpecialDayPrice { get; set; }
    
    public string CurrencyCode { get; set; } = "TRY";
    
    // Konaklama kuralları
    public int MinStayDays { get; set; } = 1;
    public int MaxStayDays { get; set; } = 30;
    
    // İptal politikası
    public string CancellationPolicy { get; set; } // "Flexible", "Moderate", "Strict"
    public int FreeCancellationDays { get; set; } = 7; // 7 gün öncesine kadar ücretsiz iptal
    public decimal? CancellationFee { get; set; } // İptal ücreti yüzdesi
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Unit Unit { get; set; }
}

