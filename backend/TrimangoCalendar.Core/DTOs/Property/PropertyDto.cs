namespace TrimangoCalendar.Core.DTOs;

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

