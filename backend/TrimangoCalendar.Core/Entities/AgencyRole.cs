namespace TrimangoCalendar.Core.Entities;

public enum AgencyRole
{
    [Description("Acente Admin")]
    Admin = 1,
    
    [Description("Yönetici")]
    Manager = 2,
    
    [Description("Rezervasyon Görevlisi")]
    Agent = 3,
    
    [Description("Sadece Görüntüleme")]
    Viewer = 4
}
