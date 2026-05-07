public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Subdomain { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string TaxNumber { get; set; } // Vergi numarası
    public string TaxOffice { get; set; } // Vergi dairesi
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Plan { get; set; } // "Free", "Pro", "Enterprise"
    public DateTime PlanStartDate { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public int MaxProperties { get; set; } = 5; // Plana göre değişir
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; }
    public ICollection<Property> Properties { get; set; }
}

