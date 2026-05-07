using System.ComponentModel;

public enum CalendarDayStatus
{
    [Description("Müsait")]
    Available = 1,
    
    [Description("Rezerve")]
    Reserved = 2,
    
    [Description("Kapalı")]
    Blocked = 3,
    
    [Description("Bakımda")]
    Maintenance = 4,
    
    [Description("Kontenjan Dolu")]
    AllotmentFull = 5
}
