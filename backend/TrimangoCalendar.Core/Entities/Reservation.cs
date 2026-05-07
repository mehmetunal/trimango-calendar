namespace TrimangoCalendar.Core.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public Guid GuestId { get; set; }

    // Rezervasyon numarası (otomatik)
    public string ReservationNumber { get; set; } // "R20241001-001"

    // Tarihler
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int TotalNights { get; set; }

    // Misafir sayıları
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Infants { get; set; }

    // Durum
    public ReservationStatus Status { get; set; }
    public string StatusNote { get; set; } // Durum değişikliği notu
    public DateTime? StatusChangedAt { get; set; }

    // Fiyat bilgileri
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string CurrencyCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string PromoCode { get; set; }

    // Özel istekler
    public string SpecialRequests { get; set; }
    public string Notes { get; set; } // Personel notları

    // Check-in/out detayları
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public bool IsLateCheckout { get; set; }

    // İptal bilgileri
    public DateTime? CancelledAt { get; set; }
    public string CancellationReason { get; set; }
    public decimal? RefundAmount { get; set; }

    // Kaynak
    public ReservationSource Source { get; set; } // Direct, Booking.com, Airbnb
    public string ExternalReference { get; set; } // Harici sistem referansı

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; }
    public Unit Unit { get; set; }
    public Guest Guest { get; set; }
    public ICollection<ReservationPayment> Payments { get; set; }
    public ICollection<ReservationHistory> History { get; set; }
    public PropertyReview Review { get; set; }
}
