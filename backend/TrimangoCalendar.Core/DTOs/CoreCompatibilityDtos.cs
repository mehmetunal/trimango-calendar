namespace TrimangoCalendar.Core.DTOs;

public class CreateAgencyDto
{
    public string Name { get; set; } = string.Empty;
}

public class AgencyPerformanceDto
{
    public Guid AgencyId { get; set; }
}

public class AvailabilitySearchDto
{
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
}

public class AvailabilityResultDto
{
    public bool Available { get; set; }
}

public class BookingWidgetDto
{
    public Guid Id { get; set; }
    public string WidgetKey { get; set; } = string.Empty;
}

public class PropertySearchResultDto
{
    public List<PropertyDto> Items { get; set; } = new();
}

public class PropertyDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CalendarBlockDto
{
    public Guid Id { get; set; }
}

public class BulkPriceDto
{
    public Guid UnitId { get; set; }
}

public class CalendarPriceDto
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
}

public class AgencyCalendarDto
{
    public Guid AgencyId { get; set; }
}

public class OwnerCalendarDto
{
    public Guid PropertyId { get; set; }
}

public class CurrencyDto
{
    public string Code { get; set; } = "TRY";
    public string Name { get; set; } = "Turkish Lira";
}

public class PropertyImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class UnitImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class NotificationTemplateDto
{
    public string Code { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

public class UpdatePropertyDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateUnitDto
{
    public string Name { get; set; } = string.Empty;
}

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ReportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
