namespace TrimangoCalendar.Core.Entities;

public class AgencyUser
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string UserId { get; set; } // Identity User ID

    // Kişisel bilgiler
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    // Yetki seviyesi (acente içinde)
    public AgencyRole Role { get; set; } // Admin, Manager, Agent

    // Durum
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Agency Agency { get; set; }
    public ApplicationUser User { get; set; }
}

