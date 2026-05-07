namespace TrimangoCalendar.Core.Entities;

public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public NotificationType Type { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SMSEnabled { get; set; } = false;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = false;

    public string EmailAddresses { get; set; } // JSON: ["admin@hotel.com", "manager@hotel.com"]
    public string PhoneNumbers { get; set; } // JSON: ["+905551234567"]

    public Tenant Tenant { get; set; }
}
