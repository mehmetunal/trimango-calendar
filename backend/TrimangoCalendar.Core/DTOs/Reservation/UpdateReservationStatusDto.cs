namespace TrimangoCalendar.Core.DTOs;

public class UpdateReservationStatusDto
{
    [Required]
    public Guid ReservationId { get; set; }

    [Required]
    public ReservationStatus NewStatus { get; set; }

    public string Note { get; set; }
}

