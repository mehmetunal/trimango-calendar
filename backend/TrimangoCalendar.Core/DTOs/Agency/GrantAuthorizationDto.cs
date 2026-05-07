namespace TrimangoCalendar.Core.DTOs;

public class GrantAuthorizationDto
{
    [Required]
    public Guid AgencyId { get; set; }
    
    [Required]
    public Guid PropertyId { get; set; }
    
    [Required]
    public AuthorizationLevel Level { get; set; }
    
    // Birim kısıtlaması
    public List<Guid> AllowedUnitIds { get; set; } // null = tümü
    
    // Yetkiler
    public bool CanViewPrices { get; set; } = true;
    public bool CanSetPrices { get; set; } = false;
    public bool CanCreateReservation { get; set; } = true;
    public bool CanModifyReservation { get; set; } = false;
    public bool CanCancelReservation { get; set; } = false;
    
    // Fiyat politikası
    public PriceDisplayType PriceDisplay { get; set; } = PriceDisplayType.Net;
    public decimal? CustomCommissionRate { get; set; }
    public decimal? MaxMarkupRate { get; set; }
    public decimal? DefaultMarkupRate { get; set; }
    
    // Kontenjan
    public bool HasAllotment { get; set; } = false;
    public int? TotalAllotment { get; set; }
    
    // Tarih aralığı
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public string Notes { get; set; }
}

