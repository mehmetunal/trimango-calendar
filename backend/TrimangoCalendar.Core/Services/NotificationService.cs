using Microsoft.Extensions.Logging;

namespace TrimangoCalendar.Core.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IMemoryCache _cache;
    
    public NotificationService(
        AppDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task SendAsync(Guid tenantId, NotificationType type, Dictionary<string, string> data,
        Guid? referenceId = null, string referenceType = null)
    {
        // Tenant'ın bildirim tercihlerini kontrol et
        var preferences = await GetPreferencesAsync(tenantId, type);
        if (preferences == null) return; // Tercih yoksa gönderme
        
        var template = await GetTemplateByTypeAsync(tenantId, type);
        if (template == null)
        {
            // Varsayılan şablonu kullan
            template = GetDefaultTemplate(type);
        }
        
        var notifications = new List<Notification>();
        
        // Email bildirimi
        if (preferences.EmailEnabled && !string.IsNullOrEmpty(preferences.EmailAddresses))
        {
            var emails = JsonSerializer.Deserialize<List<string>>(preferences.EmailAddresses);
            var body = await ProcessTemplateAsync(template.BodyTemplate, data);
            var subject = await ProcessTemplateAsync(template.Subject, data);
            
            foreach (var email in emails)
            {
                var notification = CreateNotification(tenantId, type, NotificationChannel.Email,
                    subject, body, email, referenceId, referenceType);
                notifications.Add(notification);
                
                // Hemen göndermeyi dene
                try
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Email gönderilemedi: {Email}", email);
                }
            }
        }
        
        // SMS bildirimi
        if (preferences.SMSEnabled && !string.IsNullOrEmpty(preferences.PhoneNumbers))
        {
            var phones = JsonSerializer.Deserialize<List<string>>(preferences.PhoneNumbers);
            var smsBody = await ProcessTemplateAsync(template.SMSTemplate, data);
            
            foreach (var phone in phones)
            {
                var notification = CreateNotification(tenantId, type, NotificationChannel.SMS,
                    null, smsBody, null, referenceId, referenceType);
                notification.RecipientPhone = phone;
                notifications.Add(notification);
                
                try
                {
                    await _smsService.SendSmsAsync(phone, smsBody);
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = ex.Message;
                }
            }
        }
        
        // Uygulama içi bildirim
        if (preferences.InAppEnabled)
        {
            var notification = CreateNotification(tenantId, type, NotificationChannel.InApp,
                template.Subject, template.BodyTemplate, null, referenceId, referenceType);
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            notifications.Add(notification);
        }
        
        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }
    
    private Notification CreateNotification(Guid tenantId, NotificationType type, 
        NotificationChannel channel, string title, string message, string email,
        Guid? referenceId, string referenceType)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = type,
            Channel = channel,
            Title = title,
            Message = message,
            RecipientEmail = email,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// ProcessTemplateAsync methodunu çalıştırır.
    /// </summary>
    public async Task<string> ProcessTemplateAsync(string template, Dictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        
        var result = template;
        foreach (var item in data)
        {
            result = result.Replace($"{{{{{item.Key}}}}}", item.Value);
        }
        
        return result;
    }
    
    /// <summary>
    /// GetPreferencesAsync methodunu çalıştırır.
    /// </summary>
    private async Task<NotificationPreference> GetPreferencesAsync(Guid tenantId, NotificationType type)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Type == type);
    }
    
    /// <summary>
    /// GetDefaultTemplate methodunu çalıştırır.
    /// </summary>
    private NotificationTemplate GetDefaultTemplate(NotificationType type)
    {
        return type switch
        {
            NotificationType.NewReservation => new NotificationTemplate
            {
                Subject = "Yeni Rezervasyon: {{ReservationNumber}}",
                BodyTemplate = @"
                    <h2>Yeni Rezervasyon</h2>
                    <p><strong>Rezervasyon No:</strong> {{ReservationNumber}}</p>
                    <p><strong>Misafir:</strong> {{GuestName}}</p>
                    <p><strong>Mülk:</strong> {{PropertyName}}</p>
                    <p><strong>Birim:</strong> {{UnitName}}</p>
                    <p><strong>Giriş:</strong> {{CheckInDate}}</p>
                    <p><strong>Çıkış:</strong> {{CheckOutDate}}</p>
                    <p><strong>Toplam:</strong> {{TotalAmount}} {{CurrencyCode}}</p>",
                SMSTemplate = "Yeni Rez: {{ReservationNumber}} - {{GuestName}} - {{CheckInDate}}"
            },
            NotificationType.UpcomingCheckIn => new NotificationTemplate
            {
                Subject = "Yarın Check-in: {{ReservationNumber}}",
                BodyTemplate = @"<p>{{GuestName}} yarın {{CheckInTime}}'da check-in yapacak.</p>",
                SMSTemplate = "Hatırlatma: {{GuestName}} yarın check-in - {{ReservationNumber}}"
            },
            NotificationType.NewAuthorization => new NotificationTemplate
            {
                Subject = "Yeni Yetkilendirme: {{AgencyName}}",
                BodyTemplate = @"<p>{{AgencyName}} firmasına {{PropertyName}} için yetki verildi.</p>",
                SMSTemplate = "{{AgencyName}} - {{PropertyName}} için yetkilendirildi."
            },
            _ => new NotificationTemplate
            {
                Subject = "Bildirim",
                BodyTemplate = "<p>{{Message}}</p>",
                SMSTemplate = "{{Message}}"
            }
        };
    }
}
