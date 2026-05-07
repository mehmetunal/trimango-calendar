namespace TrimangoCalendar.Core.Entities;

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
