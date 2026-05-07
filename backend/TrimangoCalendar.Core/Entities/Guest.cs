namespace TrimangoCalendar.Core.Entities;

public class Guest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    // Kişisel bilgiler
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Phone2 { get; set; }

    // Kimlik bilgileri
    public string TcKimlikNo { get; set; } // TC Kimlik No
    public string PassportNumber { get; set; }
    public string Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; }

    // Adres bilgileri
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }

    // Segmentasyon
    public string GuestType { get; set; } // "Regular", "VIP", "Corporate"
    public int TotalStays { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastStayAt { get; set; }

    // İletişim tercihleri
    public bool AllowMarketing { get; set; }
    public bool AllowSms { get; set; }
    public string PreferredLanguage { get; set; } = "tr";

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Reservation> Reservations { get; set; }
}
