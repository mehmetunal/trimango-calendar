namespace TrimangoCalendar.Core.Entities;

public class AgencyAuthorization
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid PropertyId { get; set; } // Hangi mülke erişecek
    public Guid GrantedByTenantId { get; set; } // Yetkiyi veren mülk sahibi
    
    // Yetki seviyesi
    public AuthorizationLevel Level { get; set; }
    
    // Hangi birimleri görebilir (tümü veya seçili)
    public string AllowedUnitIds { get; set; } // JSON: ["guid1", "guid2"] veya "*" hepsi
    
    // Fiyat yetkileri
    public bool CanViewPrices { get; set; } = true; // Fiyatları görebilir
    public bool CanSetPrices { get; set; } = false; // Fiyat belirleyebilir
    public bool CanCreateReservation { get; set; } = true; // Rezervasyon yapabilir
    public bool CanModifyReservation { get; set; } = false; // Rezervasyon değiştirebilir
    public bool CanCancelReservation { get; set; } = false; // İptal edebilir
    
    // Fiyat görüntüleme tipi
    public PriceDisplayType PriceDisplay { get; set; } // Net, Commission, Markup
    
    // Komisyon (acenteye özel, varsayılanı ezer)
    public decimal? CustomCommissionRate { get; set; }
    
    // Markup (acentenin üstüne koyabileceği fark)
    public decimal? MaxMarkupRate { get; set; } // Maks %20 markup
    public decimal? DefaultMarkupRate { get; set; } // Varsayılan %10 markup
    
    // Tarih kısıtlamaları
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    // Kontenjan yönetimi
    public bool HasAllotment { get; set; } // Kontenjan var mı?
    public int? TotalAllotment { get; set; } // Toplam kontenjan
    public int? UsedAllotment { get; set; } // Kullanılan kontenjan
    
    // Durum
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; }
    
    // Audit
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string GrantedBy { get; set; }
    
    // Navigation
    public Agency Agency { get; set; }
    public Property Property { get; set; }
}

