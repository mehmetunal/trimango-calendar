public class DashboardDto
{
    // Bugünkü durum
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public double CurrentOccupancy { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    
    // Genel istatistikler
    public int TotalProperties { get; set; }
    public int TotalReservations { get; set; }
    public int ActiveReservations { get; set; }
    public int PendingReservations { get; set; }
    
    // Gelir
    public decimal MonthlyRevenue { get; set; }
    public string CurrencyCode { get; set; }
    public decimal AverageRevenuePerReservation { get; set; }
    
    // Listeler
    public List<RecentReservationDto> RecentReservations { get; set; }
    
    // Grafik verileri
    public List<ChartDataPoint> OccupancyChart { get; set; }
    public List<ChartDataPoint> RevenueChart { get; set; }
    public List<TopAgencyDto> TopAgencies { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; }
    public decimal Value { get; set; }
    public string Color { get; set; }
}

public class RecentReservationDto
{
    public Guid Id { get; set; }
    public string ReservationNumber { get; set; }
    public string GuestName { get; set; }
    public string PropertyName { get; set; }
    public string UnitName { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; }
    public string Status { get; set; }
}

public class TopAgencyDto
{
    public Guid AgencyId { get; set; }
    public string AgencyName { get; set; }
    public int ReservationCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string CurrencyCode { get; set; }
}

