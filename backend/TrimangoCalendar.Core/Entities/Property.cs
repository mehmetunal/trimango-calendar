// Core/Entities/Property.cs
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

// Core/Entities/PropertyType.cs
public enum PropertyType
{
    Hotel = 1,
    ApartHotel = 2,
    Bungalov = 3,
    Villa = 4,
    Ev = 5,
    Oda = 6,
    Pansiyon = 7,
    Resort = 8,
    ButikOtel = 9,
    DagEvi = 10
}

// Core/Entities/PropertyImage.cs
public class PropertyImage
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string FilePath { get; set; }
    public string ThumbnailPath { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public int SortOrder { get; set; }
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; }
    
    public Property Property { get; set; }
}

// Core/Entities/Unit.cs
public class Unit
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Name { get; set; } // "Standart Oda", "Deluxe Suite"
    public string UnitNumber { get; set; } // "101", "A Blok-2"
    public int Floor { get; set; }
    public string Description { get; set; }
    
    // Kapasite
    public int MaxAdults { get; set; } = 2;
    public int MaxChildren { get; set; } = 0;
    public int MaxInfants { get; set; } = 0;
    public int TotalBeds { get; set; } = 1;
    
    // Yatak Tipleri (JSON)
    public string BedConfiguration { get; set; } // [{type:"Double", count:1}]
    
    // Ölçüler
    public decimal? Size { get; set; } // m²
    public string SizeUnit { get; set; } = "m²";
    
    // Manzara ve Özellikler
    public string View { get; set; } // "Deniz", "Dağ", "Şehir"
    public string RoomAmenities { get; set; } // JSON
    
    // Fiyatlandırma
    public decimal BasePrice { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public int ExtraBedCapacity { get; set; } = 0;
    public decimal? ExtraBedPrice { get; set; }
    
    // Durum
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Property Property { get; set; }
    public ICollection<SeasonRate> SeasonRates { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}

// Core/Entities/PropertyReview.cs
public class PropertyReview
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid GuestId { get; set; }
    public Guid ReservationId { get; set; }
    public int Rating { get; set; } // 1-5
    public int CleanlinessRating { get; set; } // Temizlik puanı
    public int ComfortRating { get; set; } // Konfor puanı
    public int LocationRating { get; set; } // Konum puanı
    public int StaffRating { get; set; } // Personel puanı
    public string Comment { get; set; }
    public string Response { get; set; } // İşletme cevabı
    public DateTime? ResponseDate { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Property Property { get; set; }
    public Guest Guest { get; set; }
}