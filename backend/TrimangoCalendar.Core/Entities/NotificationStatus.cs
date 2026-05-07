public enum NotificationStatus
{
    [Description("Beklemede")]
    Pending = 1,
    
    [Description("Gönderildi")]
    Sent = 2,
    
    [Description("Okundu")]
    Read = 3,
    
    [Description("Hata")]
    Failed = 4,
    
    [Description("İptal Edildi")]
    Cancelled = 5
}

