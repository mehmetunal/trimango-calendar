// Core/DTOs/Property/CreatePropertyDto.cs
public class CreatePropertyDto
{
    [Required(ErrorMessage = "Mülk tipi zorunludur")]
    public PropertyType Type { get; set; }
    
    [Required(ErrorMessage = "Mülk adı zorunludur")]
    [StringLength(300, MinimumLength = 3)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [StringLength(200)]
    public string ShortDescription { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    [Phone]
    public string Phone { get; set; }
    
    public string Website { get; set; }
    
    [Required(ErrorMessage = "Adres zorunludur")]
    public string Address { get; set; }
    
    public string District { get; set; }
    
    [Required(ErrorMessage = "Şehir zorunludur")]
    public string City { get; set; }
    
    public string Country { get; set; } = "Türkiye";
    public string PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string CheckInTime { get; set; } = "14:00";
    public string CheckOutTime { get; set; } = "12:00";
    
    public List<string> Amenities { get; set; } = new();
}

// Core/DTOs/Property/PropertyDto.cs
public class PropertyDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; }
    public string Type { get; set; }
    public string TypeName { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public string ShortDescription { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string District { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string CheckInTime { get; set; }
    public string CheckOutTime { get; set; }
    public string CoverImageUrl { get; set; }
    public List<string> Amenities { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalUnitCount { get; set; }
    public decimal StartingPrice { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Core/DTOs/Property/PropertySearchDto.cs
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

// Core/DTOs/Unit/CreateUnitDto.cs
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