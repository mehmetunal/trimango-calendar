namespace TrimangoCalendar.Core.Entities;

public class Property
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public PropertyType Type { get; set; } // Enum
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public string ShortDescription { get; set; } // Kısa açıklama (listelemelerde)
    
    // İletişim Bilgileri
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Website { get; set; }
    
    // Adres Bilgileri
    public string Address { get; set; }
    public string District { get; set; } // İlçe
    public string City { get; set; } // İl
    public string Country { get; set; } = "Türkiye";
    public string PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Konaklama Politikaları
    public TimeSpan CheckInTime { get; set; } = new(14, 0, 0);
    public TimeSpan CheckOutTime { get; set; } = new(12, 0, 0);
    public int MinStayDays { get; set; } = 1;
    public int MaxStayDays { get; set; } = 30;
    
    // Özellikler ve Hizmetler (JSON)
    public string Amenities { get; set; } // ["WiFi", "Havuz", "Otopark"]
    public string Rules { get; set; } // Kurallar JSON
    public string CancellationPolicy { get; set; } // İptal politikası
    
    // Görsel
    public string CoverImage { get; set; } // Kapak fotoğrafı
    
    // İstatistikler (Denormalize)
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalUnitCount { get; set; }
    
    // Durum
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; }
    public ICollection<Unit> Units { get; set; }
    public ICollection<PropertyImage> Images { get; set; }
    public ICollection<PropertyReview> Reviews { get; set; }
}

