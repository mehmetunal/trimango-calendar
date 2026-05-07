namespace TrimangoCalendar.Core.Entities;

public enum AuthorizationLevel
{
    [Description("Sadece Görüntüleme")]
    ViewOnly = 1,

    [Description("Fiyat ve Müsaitlik")]
    PriceAndAvailability = 2,

    [Description("Rezervasyon Yapabilir")]
    CanReserve = 3,

    [Description("Tam Yetki")]
    FullAccess = 4
}

