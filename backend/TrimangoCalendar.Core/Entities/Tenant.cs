// Core/Entities/Tenant.cs
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

// Data/Configurations/TenantConfiguration.cs
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(t => t.Subdomain)
            .IsUnique();
        
        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(t => t.Plan)
            .HasMaxLength(20)
            .HasDefaultValue("Free");
        
        builder.Property(t => t.MaxProperties)
            .HasDefaultValue(5);
    }
}