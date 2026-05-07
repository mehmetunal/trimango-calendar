namespace TrimangoCalendar.Core.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }
    public string ReservationNumber { get; set; }
    
    // Mülk bilgisi
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    
    // Misafir bilgisi
    public Guid GuestId { get; set; }
    public string GuestName { get; set; }
    public string GuestEmail { get; set; }
    public string GuestPhone { get; set; }
    
    // Tarihler
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int TotalNights { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    
    // Misafir sayıları
    public int Adults { get; set; }
    public int Children { get; set; }
    
    // Durum
    public string Status { get; set; }
    public string StatusDescription { get; set; }
    public string StatusColor { get; set; }
    
    // Fiyat
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string CurrencyCode { get; set; }
    public string FormattedTotal { get; set; }
    public string FormattedRemaining { get; set; }
    
    // Kaynak
    public string Source { get; set; }
    public string ExternalReference { get; set; }
    
    // İptal
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}

