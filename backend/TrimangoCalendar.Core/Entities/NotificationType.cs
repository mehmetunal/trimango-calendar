namespace TrimangoCalendar.Core.Entities;

public enum NotificationType
{
    // Rezervasyon bildirimleri
    [Description("Yeni Rezervasyon")]
    NewReservation = 1,

    [Description("Rezervasyon Onayı")]
    ReservationConfirmed = 2,

    [Description("Rezervasyon İptali")]
    ReservationCancelled = 3,

    [Description("Yaklaşan Check-in")]
    UpcomingCheckIn = 4,

    [Description("Check-in Yapıldı")]
    CheckedIn = 5,

    [Description("Check-out Yapıldı")]
    CheckedOut = 6,

    // Yetkilendirme bildirimleri
    [Description("Yeni Yetkilendirme")]
    NewAuthorization = 10,

    [Description("Yetki İptali")]
    AuthorizationRevoked = 11,

    [Description("Yetki Güncelleme")]
    AuthorizationUpdated = 12,

    // Fiyat bildirimleri
    [Description("Fiyat Değişikliği")]
    PriceChanged = 20,

    [Description("Sezon Başlangıcı")]
    SeasonStarted = 21,

    // Sistem bildirimleri
    [Description("Kontenjan Uyarısı")]
    AllotmentWarning = 30,

    [Description("Bakım Hatırlatma")]
    MaintenanceReminder = 31,

    [Description("Ödeme Hatırlatma")]
    PaymentReminder = 40,

    // Değerlendirme
    [Description("Değerlendirme İsteği")]
    ReviewRequest = 50
}

