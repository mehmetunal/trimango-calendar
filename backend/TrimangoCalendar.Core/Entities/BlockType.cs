namespace TrimangoCalendar.Core.Entities;

public enum BlockType
{
    [Description("Bakım/Onarım")]
    Maintenance = 1,
    
    [Description("Kapalı Sezon")]
    ClosedSeason = 2,
    
    [Description("Özel Kullanım")]
    PrivateUse = 3,
    
    [Description("Kontenjan Doldu")]
    AllotmentFull = 4,
    
    [Description("Diğer")]
    Other = 99
}

