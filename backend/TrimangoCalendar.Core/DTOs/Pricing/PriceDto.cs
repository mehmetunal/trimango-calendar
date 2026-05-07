namespace TrimangoCalendar.Core.DTOs;

public class PriceDto
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string FormattedPrice { get; set; }
    public decimal? OriginalAmount { get; set; } // Dönüşüm öncesi tutar
    public string OriginalCurrency { get; set; }
}

