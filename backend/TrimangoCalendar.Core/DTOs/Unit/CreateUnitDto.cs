public class CreateUnitDto
{
    [Required]
    public string Name { get; set; }
    
    public string UnitNumber { get; set; }
    public int Floor { get; set; }
    public string Description { get; set; }
    
    [Range(1, 20)]
    public int MaxAdults { get; set; } = 2;
    
    [Range(0, 10)]
    public int MaxChildren { get; set; } = 0;
    
    [Range(0, 5)]
    public int MaxInfants { get; set; } = 0;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }
    
    public string CurrencyCode { get; set; } = "TRY";
    public decimal? Size { get; set; }
    public string View { get; set; }
    public List<string> RoomAmenities { get; set; } = new();
}
