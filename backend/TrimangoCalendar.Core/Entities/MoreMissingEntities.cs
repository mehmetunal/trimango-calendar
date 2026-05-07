namespace TrimangoCalendar.Core.Entities;

public class TenantSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Timezone { get; set; } = "Europe/Istanbul";
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public string DefaultCurrency { get; set; } = "TRY";
    public string DefaultLanguage { get; set; } = "tr";
    public Tenant? Tenant { get; set; }
}

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public int MaxProperties { get; set; }
    public int MaxUsers { get; set; }
    public int MaxAgencies { get; set; }
    public bool IsActive { get; set; }
}

public class TenantSubscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string Status { get; set; } = "Active";
    public string? PaymentMethod { get; set; }
    public Tenant? Tenant { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}

public class UnitAmenity
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UnitImage
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Url { get; set; } = string.Empty;
}

public partial class ReservationPayment
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime PaidAt { get; set; }
}

public partial class GuestDocument
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
}
