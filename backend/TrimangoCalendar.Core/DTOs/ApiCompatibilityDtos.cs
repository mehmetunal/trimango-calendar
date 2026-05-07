namespace TrimangoCalendar.Core.DTOs;

public class ReportRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? AgencyId { get; set; }
}

public class CreateWidgetDto
{
    public Guid PropertyId { get; set; }
    public string? Name { get; set; }
}

public class UpdateWidgetDto
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BookingSearchDto
{
    public Guid PropertyId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; }
    public string? CurrencyCode { get; set; }
}

public class CreateBookingDto
{
    public Guid UnitId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}

public class UpdateAuthorizationDto
{
    public AuthorizationLevel Level { get; set; }
}

public class BlockDatesDto
{
    public Guid UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public Guid CreatedByTenantId { get; set; }
}

public class CreateAgencyReservationDto
{
    public Guid AuthorizationId { get; set; }
    public Guid UnitId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class SetDailyPriceDto
{
    public Guid UnitId { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public Guid SetByAgencyId { get; set; }
    public PriceSource Source { get; set; } = PriceSource.Manual;
}
