public class UnitAvailabilityDto
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public string UnitNumber { get; set; }
    public int MaxAdults { get; set; }
    public decimal BasePrice { get; set; }
    public string CurrencyCode { get; set; }
    public List<DateAvailabilityDto> AvailableDates { get; set; }
}

public class DateAvailabilityDto
{
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public Guid? ReservationId { get; set; }
    public string ReservationNumber { get; set; }
}

public class ReservationStatsDto
{
    public int TotalReservations { get; set; }
    public int ActiveReservations { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public decimal TotalRevenue { get; set; }
    public string CurrencyCode { get; set; }
    public double OccupancyRate { get; set; }
    public int CancelledReservations { get; set; }
    public decimal CancellationRate { get; set; }
}
