namespace TrimangoCalendar.Core.Entities;

public enum ReservationStatus
{
    [Description("Beklemede")]
    Pending = 1,
    
    [Description("Onaylandı")]
    Confirmed = 2,
    
    [Description("Ödeme Bekliyor")]
    AwaitingPayment = 3,
    
    [Description("Giriş Yapıldı")]
    CheckedIn = 4,
    
    [Description("Çıkış Yapıldı")]
    CheckedOut = 5,
    
    [Description("İptal Edildi")]
    Cancelled = 6,
    
    [Description("Gelmedi")]
    NoShow = 7,
    
    [Description("Tamamlandı")]
    Completed = 8
}

