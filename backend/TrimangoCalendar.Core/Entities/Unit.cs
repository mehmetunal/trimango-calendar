namespace TrimangoCalendar.Core.Entities;

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

