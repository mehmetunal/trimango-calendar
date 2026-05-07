public enum PriceDisplayType
{
    [Description("Net Fiyat")]
    Net = 1, // Doğrudan oda fiyatı
    
    [Description("Komisyon Dahil")]
    Commission = 2, // Net + komisyon
    
    [Description("Markup Fiyat")]
    Markup = 3 // Acentenin kendi fiyatı
}

