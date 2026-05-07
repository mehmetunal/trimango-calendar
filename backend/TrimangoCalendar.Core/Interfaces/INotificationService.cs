namespace TrimangoCalendar.Core.Interfaces;

public interface INotificationService
{
    Task SendAsync(Guid tenantId, NotificationType type, Dictionary<string, string> data,
        Guid? referenceId = null, string referenceType = null);

    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string to, string message);
    Task SendBulkAsync(List<Notification> notifications);

    Task<List<NotificationDto>> GetNotificationsAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(Guid tenantId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid tenantId);

    // Şablon yönetimi
    Task<NotificationTemplateDto> GetTemplateAsync(string code);
    Task<string> ProcessTemplateAsync(string template, Dictionary<string, string> data);
}

