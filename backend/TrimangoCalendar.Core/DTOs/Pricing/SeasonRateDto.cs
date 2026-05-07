public class CreateSeasonRateDto
{
    [Required]
    public Guid UnitId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal WeekdayPrice { get; set; }
    
    public decimal? WeekendPrice { get; set; }
    public decimal? SpecialDayPrice { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public int MinStayDays { get; set; } = 1;
}

public class SeasonRateDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal WeekdayPrice { get; set; }
    public decimal? WeekendPrice { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsActive { get; set; }
}
