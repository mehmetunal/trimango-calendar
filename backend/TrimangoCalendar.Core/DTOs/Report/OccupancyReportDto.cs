namespace TrimangoCalendar.Core.DTOs;

public class OccupancyReportDto
{
    public Guid? PropertyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalUnits { get; set; }
    public double AverageOccupancyRate { get; set; }
    public DateTime? PeakOccupancyDate { get; set; }
    public DateTime? LowestOccupancyDate { get; set; }
    public List<DailyOccupancyDto> DailyOccupancy { get; set; }
}

public class DailyOccupancyDto
{
    public DateTime Date { get; set; }
    public int TotalUnits { get; set; }
    public int ReservedUnits { get; set; }
    public double OccupancyRate { get; set; }
}

