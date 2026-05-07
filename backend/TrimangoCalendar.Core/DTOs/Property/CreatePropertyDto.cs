namespace TrimangoCalendar.Core.DTOs;

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

