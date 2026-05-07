namespace TrimangoCalendar.Core.Entities;

public class Agency
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // Acente de bir tenant
    
    // Firma bilgileri
    public string CompanyName { get; set; }
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; } = "Türkiye";
    
    // İletişim
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Website { get; set; }
    
    // Yetkili kişi
    public string ContactPerson { get; set; }
    public string ContactPhone { get; set; }
    public string ContactEmail { get; set; }
    
    // Acente tipi
    public AgencyType Type { get; set; } // TravelAgency, TourOperator, OTA, Corporate
    
    // Komisyon oranı (varsayılan)
    public decimal DefaultCommissionRate { get; set; } // %10 = 10
    
    // Durum
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } // Onaylı acente mi?
    public string Notes { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; }
    public ICollection<AgencyAuthorization> Authorizations { get; set; }
    public ICollection<AgencyUser> Users { get; set; }
}

