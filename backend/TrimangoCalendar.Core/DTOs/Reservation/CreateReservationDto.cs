namespace TrimangoCalendar.Core.DTOs;

public class CreateReservationDto
{
    [Required]
    public Guid UnitId { get; set; }
    
    // Misafir bilgileri
    [Required(ErrorMessage = "Ad zorunludur")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Soyad zorunludur")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Email zorunludur")]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Telefon zorunludur")]
    [Phone]
    public string Phone { get; set; }
    
    // Tarihler
    [Required]
    [FutureDate(ErrorMessage = "Giriş tarihi bugünden sonra olmalıdır")]
    public DateTime CheckIn { get; set; }
    
    [Required]
    [DateGreaterThan("CheckIn", ErrorMessage = "Çıkış tarihi giriş tarihinden sonra olmalıdır")]
    public DateTime CheckOut { get; set; }
    
    // Misafir sayıları
    [Required]
    [Range(1, 20)]
    public int Adults { get; set; } = 1;
    
    [Range(0, 10)]
    public int Children { get; set; } = 0;
    
    [Range(0, 5)]
    public int Infants { get; set; } = 0;
    
    // Fiyat ve ödeme
    public string CurrencyCode { get; set; } = "TRY";
    public string PromoCode { get; set; }
    
    // Ek bilgiler
    [MaxLength(1000)]
    public string SpecialRequests { get; set; }
    
    public ReservationSource Source { get; set; } = ReservationSource.Website;
    
    // Misafir ek bilgileri (opsiyonel)
    public string TcKimlikNo { get; set; }
    public string PassportNumber { get; set; }
    public string Nationality { get; set; }
}

