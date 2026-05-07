namespace TrimangoCalendar.Core.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string Code { get; set; } // "NEW_RESERVATION_EMAIL"
    public string Name { get; set; } // "Yeni Rezervasyon Email Şablonu"
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }

    public string Subject { get; set; } // Email konusu
    public string BodyTemplate { get; set; } // HTML şablon
    public string SMSTemplate { get; set; } // SMS şablonu

    // Değişkenler: {{GuestName}}, {{ReservationNumber}}, {{CheckInDate}}
    public string AvailableVariables { get; set; } // JSON

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

