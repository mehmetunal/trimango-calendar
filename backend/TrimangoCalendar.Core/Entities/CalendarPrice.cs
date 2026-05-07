public class CalendarPrice
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    
    // Kim belirledi?
    public Guid? SetByTenantId { get; set; } // Mülk sahibi
    public Guid? SetByAgencyId { get; set; } // Acente (eğer yetkisi varsa)
    
    // Fiyat tipi
    public PriceSource Source { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public Unit Unit { get; set; }
}

