namespace TrimangoCalendar.Core.DTOs;

public class ExchangeRateDto
{
    public string BaseCurrency { get; set; } = "TRY";
    public string TargetCurrency { get; set; } = "USD";
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
}
