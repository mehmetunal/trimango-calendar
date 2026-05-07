public class CreateTenantDto
{
    [Required(ErrorMessage = "Firma adı zorunludur")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Email adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Telefon zorunludur")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string Phone { get; set; }
    
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Subdomain { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Plan { get; set; }
    public int PropertyCount { get; set; }
    public int ActiveReservationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateTenantDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [Phone]
    public string Phone { get; set; }
    
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

public class ChangePlanDto
{
    [Required]
    public string NewPlan { get; set; } // "Free", "Pro", "Enterprise"
    
    [Required]
    public Guid TenantId { get; set; }
}
