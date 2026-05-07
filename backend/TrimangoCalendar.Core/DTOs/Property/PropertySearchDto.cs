namespace TrimangoCalendar.Core.DTOs;

public class PropertySearchDto
{
    public string City { get; set; }
    public string Country { get; set; }
    public PropertyType? Type { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int? Adults { get; set; }
    public int? Children { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public List<string> Amenities { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt"; // Price, Rating, Name
    public bool SortDescending { get; set; } = true;
}

