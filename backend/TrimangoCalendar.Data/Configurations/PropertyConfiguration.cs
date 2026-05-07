// Data/Configurations/PropertyConfiguration.cs
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.HasIndex(p => new { p.TenantId, p.Slug })
            .IsUnique();
        
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(p => p.Email)
            .HasMaxLength(256);
        
        builder.Property(p => p.Phone)
            .HasMaxLength(20);
        
        builder.Property(p => p.Address)
            .HasMaxLength(500);
        
        builder.Property(p => p.City)
            .HasMaxLength(100);
        
        builder.HasIndex(p => new { p.City, p.IsActive });
        builder.HasIndex(p => new { p.TenantId, p.Type });
        
        // Global filter
        builder.HasQueryFilter(p => !p.IsDeleted);
        
        builder.HasOne(p => p.Tenant)
            .WithMany(t => t.Properties)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// Data/Configurations/UnitConfiguration.cs
public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(u => u.UnitNumber)
            .HasMaxLength(50);
        
        builder.HasIndex(u => new { u.PropertyId, u.UnitNumber })
            .IsUnique();
        
        builder.Property(u => u.BasePrice)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(u => u.CurrencyCode)
            .HasMaxLength(3)
            .HasDefaultValue("TRY");
        
        builder.HasOne(u => u.Property)
            .WithMany(p => p.Units)
            .HasForeignKey(u => u.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}