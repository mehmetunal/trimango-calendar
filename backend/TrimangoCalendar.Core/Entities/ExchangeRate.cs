public class ExchangeRate
{
    public long Id { get; set; }
    public string BaseCurrencyCode { get; set; }
    public string TargetCurrencyCode { get; set; }
    public decimal Rate { get; set; }
    public decimal BuyRate { get; set; } // Alış kuru
    public decimal SellRate { get; set; } // Satış kuru
    public DateTime Date { get; set; }
    public string Source { get; set; } = "TCMB"; // "TCMB", "Manual", "Fixer.io"
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Currency BaseCurrency { get; set; }
    public Currency TargetCurrency { get; set; }
}

