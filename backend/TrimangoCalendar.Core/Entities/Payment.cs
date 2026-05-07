namespace TrimangoCalendar.Core.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime PaidAt { get; set; }
}
