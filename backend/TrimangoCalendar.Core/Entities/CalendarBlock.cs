namespace TrimangoCalendar.Core.Entities;

public class CalendarBlock
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public Guid? PropertyId { get; set; } // Tüm birimler için blokaj

    // Blokaj tipi
    public BlockType Type { get; set; }

    // Tarih aralığı
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Açıklama
    public string Reason { get; set; }
    public string Notes { get; set; }

    // Kim yaptı?
    public Guid? CreatedByTenantId { get; set; } // Mülk sahibi
    public Guid? CreatedByAgencyId { get; set; } // Acente

    // Durum
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Unit Unit { get; set; }
    public Property Property { get; set; }
}

