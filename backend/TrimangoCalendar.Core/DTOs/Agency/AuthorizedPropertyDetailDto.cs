public class AuthorizedPropertyDetailDto
{
    public Guid AuthorizationId { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string PropertyDescription { get; set; }
    public string PropertyType { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string CheckInTime { get; set; }
    public string CheckOutTime { get; set; }
    
    // Yetkiler
    public string AuthorizationLevel { get; set; }
    public bool CanViewPrices { get; set; }
    public bool CanSetPrices { get; set; }
    public bool CanCreateReservation { get; set; }
    public bool CanModifyReservation { get; set; }
    public bool CanCancelReservation { get; set; }
    
    // Fiyat
    public string PriceDisplay { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal? DefaultMarkupRate { get; set; }
    public decimal? MaxMarkupRate { get; set; }
    
    // Kontenjan
    public bool HasAllotment { get; set; }
    public int? TotalAllotment { get; set; }
    public int UsedAllotment { get; set; }
    
    // Birimler
    public List<AuthorizedUnitDto> Units { get; set; }
    
    // Tarih kısıtı
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class AuthorizedUnitDto
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public string UnitNumber { get; set; }
    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public decimal? BasePrice { get; set; } // null = fiyat gösterilmez
    public string CurrencyCode { get; set; }
    public bool IsActive { get; set; }
}
