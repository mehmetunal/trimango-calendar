// backend/TrimangoCalendar.Core/DTOs/SeasonRateDtos.cs
namespace TrimangoCalendar.Core.DTOs
{
    public class SeasonRateDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal? WeekendPrice { get; set; }
        public decimal? SpecialDayPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int MinStayDays { get; set; }
        public int MaxStayDays { get; set; }
        public string CancellationPolicy { get; set; }
        public int FreeCancellationDays { get; set; }
        public decimal? CancellationFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSeasonRateDto
    {
        public Guid UnitId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal? WeekendPrice { get; set; }
        public decimal? SpecialDayPrice { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public int MinStayDays { get; set; } = 1;
        public int MaxStayDays { get; set; } = 30;
        public string CancellationPolicy { get; set; } = "Flexible";
        public int FreeCancellationDays { get; set; } = 7;
        public decimal? CancellationFee { get; set; }
    }

    public class UpdateSeasonRateDto
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? WeekdayPrice { get; set; }
        public decimal? WeekendPrice { get; set; }
        public decimal? SpecialDayPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int? MinStayDays { get; set; }
        public int? MaxStayDays { get; set; }
        public string CancellationPolicy { get; set; }
        public int? FreeCancellationDays { get; set; }
        public decimal? CancellationFee { get; set; }
    }

    public class DailyPriceDto
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsSpecialDay { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ActualPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string SeasonName { get; set; }
    }

    public class SetSpecialDayPriceDto
    {
        public Guid UnitId { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
    }

    public class SpecialDayRateDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; }
    }
}