public class ReservationHistory
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public ReservationStatus OldStatus { get; set; }
    public ReservationStatus NewStatus { get; set; }
    public string Note { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    
    public Reservation Reservation { get; set; }
}

