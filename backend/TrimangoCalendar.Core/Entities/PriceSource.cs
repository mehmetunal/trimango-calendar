namespace TrimangoCalendar.Core.Entities;

public enum PriceSource
{
    [Description("Baz Fiyat")]
    BasePrice = 1,
    
    [Description("Sezon Fiyatı")]
    SeasonRate = 2,
    
    [Description("Acente Fiyatı")]
    AgencyPrice = 3,
    
    [Description("Manuel")]
    Manual = 4
}
