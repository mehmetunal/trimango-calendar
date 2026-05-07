namespace TrimangoCalendar.Core.Entities;

public class Currency
{
    public string Code { get; set; } // "TRY", "USD", "EUR", "GBP"
    public string Symbol { get; set; } // "₺", "$", "€", "£"
    public string Name { get; set; } // "Türk Lirası", "Amerikan Doları"
    public int DecimalPlaces { get; set; } = 2;
    public string CultureCode { get; set; } // "tr-TR", "en-US"
    public bool IsBaseCurrency { get; set; } // TRY varsayılan ana para birimi
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<ExchangeRate> BaseRates { get; set; }
    public ICollection<ExchangeRate> TargetRates { get; set; }
}

