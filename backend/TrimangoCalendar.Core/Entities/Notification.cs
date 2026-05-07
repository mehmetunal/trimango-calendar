namespace TrimangoCalendar.Core.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // Bildirimin sahibi
    public Guid? AgencyId { get; set; } // Acente bildirimi ise

    // Bildirim tipi
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; } // Email, SMS, InApp, Push

    // İçerik
    public string Title { get; set; }
    public string Message { get; set; }
    public string TemplateCode { get; set; } // Email şablon kodu

    // Hedef
    public string RecipientEmail { get; set; }
    public string RecipientPhone { get; set; }
    public Guid? RecipientUserId { get; set; }

    // İlişkili veri
    public Guid? ReferenceId { get; set; } // Rezervasyon ID, Ödeme ID vb.
    public string ReferenceType { get; set; } // "Reservation", "Payment", "Authorization"

    // Durum
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string ErrorMessage { get; set; }

    // Zamanlama
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; }
}

