namespace TrimangoCalendar.Core.Entities;

public class Promotion
{
    public Guid Id { get; set; }
    public Guid? PropertyId { get; set; } // Tüm mülke indirim
    public Guid? UnitId { get; set; } // Belirli birime indirim
    public string Code { get; set; } // İndirim kodu: "ERKENREZERVASYON", "YAZ2024"
    public string Name { get; set; }
    public string Description { get; set; }
    
    public PromotionType Type { get; set; } // Percentage, FixedAmount
    public decimal DiscountValue { get; set; } // 20 (%20 indirim) veya 100 (100 TL indirim)
    public string CurrencyCode { get; set; } = "TRY";
    
    public int MinStayDays { get; set; }
    public int MaxUsageCount { get; set; } // Kaç kez kullanılabilir
    public int UsedCount { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Property Property { get; set; }
    public Unit Unit { get; set; }
}

public enum PromotionType
{
    Percentage = 1,  // Yüzde indirim
    FixedAmount = 2  // Sabit tutar indirim
}
