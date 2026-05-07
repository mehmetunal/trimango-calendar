using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrimangoCalendar.Core.Entities;

namespace TrimangoCalendar.Data.Configurations;

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