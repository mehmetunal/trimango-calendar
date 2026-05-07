namespace TrimangoCalendar.Core.Entities;

public enum ReportType
{
    [Description("Doluluk Raporu")]
    Occupancy = 1,
    
    [Description("Gelir Raporu")]
    Revenue = 2,
    
    [Description("Rezervasyon Raporu")]
    Reservation = 3,
    
    [Description("Misafir Raporu")]
    Guest = 4,
    
    [Description("Acente Performans")]
    AgencyPerformance = 5,
    
    [Description("Fiyat Karşılaştırma")]
    PriceComparison = 6,
    
    [Description("Vergi Raporu")]
    Tax = 7,
    
    [Description("Özet Rapor")]
    Summary = 8
}

public enum ReportPeriod
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Yearly = 5,
    Custom = 99
}

public enum ReportStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
