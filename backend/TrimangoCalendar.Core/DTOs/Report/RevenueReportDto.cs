namespace TrimangoCalendar.Core.DTOs;

public class RevenueReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public string CurrencyCode { get; set; }
    public int TotalReservations { get; set; }
    public decimal AverageRevenuePerDay { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalServiceFee { get; set; }
    public decimal TotalDiscounts { get; set; }

    public List<CurrencyRevenueDto> RevenueByCurrency { get; set; }
    public List<PropertyRevenueDto> RevenueByProperty { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
}

public class PropertyRevenueDto
{
    public string PropertyName { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ReservationCount { get; set; }
    public decimal AveragePerReservation { get; set; }
    public string CurrencyCode { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM", new CultureInfo("tr-TR"));
    public decimal TotalRevenue { get; set; }
    public int ReservationCount { get; set; }
    public string CurrencyCode { get; set; }
}

public class CurrencyRevenueDto
{
    public string CurrencyCode { get; set; }
    public decimal TotalRevenue { get; set; }
    public int Count { get; set; }
}
