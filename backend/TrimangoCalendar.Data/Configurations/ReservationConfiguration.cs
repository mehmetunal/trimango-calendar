public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        
        // ReservationNumber unique index
        builder.HasIndex(r => r.ReservationNumber)
            .IsUnique();
        
        // Performans için composite index'ler
        builder.HasIndex(r => new { r.UnitId, r.CheckIn, r.CheckOut });
        builder.HasIndex(r => new { r.TenantId, r.Status });
        builder.HasIndex(r => new { r.GuestId, r.CreatedAt });
        builder.HasIndex(r => r.CheckIn);
        builder.HasIndex(r => r.CheckOut);
        
        // Properties
        builder.Property(r => r.ReservationNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(r => r.TotalAmount)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(r => r.PaidAmount)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(r => r.TaxAmount)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(r => r.CurrencyCode)
            .HasMaxLength(3);
        
        builder.Property(r => r.SpecialRequests)
            .HasMaxLength(2000);
        
        // Relationships
        builder.HasOne(r => r.Unit)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(r => r.Guest)
            .WithMany(g => g.Reservations)
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

