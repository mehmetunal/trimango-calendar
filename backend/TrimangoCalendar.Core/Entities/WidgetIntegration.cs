namespace TrimangoCalendar.Core.Entities;

public class WidgetIntegration
{
    public Guid Id { get; set; }
    public Guid WidgetId { get; set; }
    public Guid BookingWidgetId { get; set; }
    public string Domain { get; set; } // Hangi domain'de çalışacak
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public BookingWidget Widget { get; set; }
}
