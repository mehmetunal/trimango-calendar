namespace TrimangoCalendar.Core.DTOs;

public class AuthorizedPropertyDto
{
    public Guid AuthorizationId { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public string City { get; set; }
    public int TotalUnits { get; set; }
    public int ActiveReservations { get; set; }
    public bool CanCreateReservation { get; set; }
    public bool CanSetPrices { get; set; }
    public string PriceDisplay { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal? DefaultMarkupRate { get; set; }
    public int? RemainingAllotment { get; set; }
    public bool IsActive { get; set; }
}

