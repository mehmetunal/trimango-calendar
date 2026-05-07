namespace TrimangoCalendar.Core.DTOs;

public class ReservationFilterDto
{
    public DateTime? CheckInFrom { get; set; }
    public DateTime? CheckInTo { get; set; }
    public DateTime? CheckOutFrom { get; set; }
    public DateTime? CheckOutTo { get; set; }
    public ReservationStatus? Status { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public Guid? GuestId { get; set; }
    public string SearchTerm { get; set; } // Misafir adı, rezervasyon no
    public ReservationSource? Source { get; set; }
    public string CurrencyCode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

