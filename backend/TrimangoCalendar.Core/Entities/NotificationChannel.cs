namespace TrimangoCalendar.Core.Entities;

public enum NotificationChannel
{
    [Description("Email")]
    Email = 1,

    [Description("SMS")]
    SMS = 2,

    [Description("Uygulama İçi")]
    InApp = 3,

    [Description("Push Bildirim")]
    Push = 4
}

