namespace TrimangoCalendar.Core.Entities;

public class BookingWidget
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }

    // Widget ayarları
    public string WidgetKey { get; set; } // Unique key: "WIDGET-ABC123"
    public string Theme { get; set; } = "default"; // "default", "modern", "minimal"
    public string PrimaryColor { get; set; } = "#2563EB"; // Ana renk
    public string SecondaryColor { get; set; } = "#1E40AF";
    public string FontFamily { get; set; } = "Inter, sans-serif";

    // Görünüm ayarları
    public bool ShowPropertyImages { get; set; } = true;
    public bool ShowAmenities { get; set; } = true;
    public bool ShowReviews { get; set; } = true;
    public bool ShowPriceBreakdown { get; set; } = true;

    // Konumlandırma
    public WidgetPosition Position { get; set; } = WidgetPosition.Right; // Sol, Sağ
    public string CustomCSS { get; set; }

    // SEO & Paylaşım
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string SharingImage { get; set; }

    // Rezervasyon ayarları
    public bool RequirePayment { get; set; } = false; // Ödeme zorunlu mu?
    public int MinAdvanceDays { get; set; } = 0; // Kaç gün önceden rezervasyon yapılabilir
    public int MaxAdvanceDays { get; set; } = 365; // En fazla kaç gün ileri rezervasyon

    // Dil ayarları
    public string DefaultLanguage { get; set; } = "tr";
    public string AvailableLanguages { get; set; } = "[\"tr\", \"en\"]"; // JSON

    // Durum
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Property Property { get; set; }
    public ICollection<WidgetIntegration> Integrations { get; set; }
}

