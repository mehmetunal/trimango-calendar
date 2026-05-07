namespace TrimangoCalendar.Core.DTOs;

public class AuthorizationDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string AgencyName { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public string Level { get; set; }
    public string LevelDescription { get; set; }

    public bool CanViewPrices { get; set; }
    public bool CanSetPrices { get; set; }
    public bool CanCreateReservation { get; set; }
    public bool CanModifyReservation { get; set; }
    public bool CanCancelReservation { get; set; }

    public string PriceDisplay { get; set; }
    public decimal? CustomCommissionRate { get; set; }
    public decimal? DefaultMarkupRate { get; set; }

    public bool HasAllotment { get; set; }
    public int? TotalAllotment { get; set; }
    public int? UsedAllotment { get; set; }
    public int RemainingAllotment => (TotalAllotment ?? 0) - (UsedAllotment ?? 0);

    public bool IsActive { get; set; }
    public DateTime GrantedAt { get; set; }
}
