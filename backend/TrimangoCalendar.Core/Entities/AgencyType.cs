public enum AgencyType
{
    [Description("Seyahat Acentası")]
    TravelAgency = 1,
    
    [Description("Tur Operatörü")]
    TourOperator = 2,
    
    [Description("Online Seyahat Acentası (OTA)")]
    OTA = 3,
    
    [Description("Kurumsal Firma")]
    Corporate = 4,
    
    [Description("Etkinlik Organizatörü")]
    EventOrganizer = 5,
    
    [Description("Diğer")]
    Other = 99
}

