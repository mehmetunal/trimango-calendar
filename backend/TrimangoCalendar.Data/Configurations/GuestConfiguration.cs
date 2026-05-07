using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrimangoCalendar.Core.Entities;

namespace TrimangoCalendar.Data.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.HasIndex(g => new { g.TenantId, g.Email });
        builder.HasIndex(g => new { g.TenantId, g.Phone });
        builder.HasIndex(g => g.TcKimlikNo);
        builder.HasIndex(g => g.PassportNumber);
        
        builder.Property(g => g.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(g => g.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(g => g.Email)
            .HasMaxLength(256);
        
        builder.Property(g => g.Phone)
            .HasMaxLength(20);
        
        builder.Property(g => g.TcKimlikNo)
            .HasMaxLength(11);
        
        builder.Property(g => g.TotalSpent)
            .HasColumnType("decimal(18,2)");
    }
}