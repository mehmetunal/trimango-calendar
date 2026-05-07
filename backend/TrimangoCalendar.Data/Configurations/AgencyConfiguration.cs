public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasIndex(a => new { a.TenantId, a.TaxNumber }).IsUnique();
        
        builder.Property(a => a.CompanyName)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(a => a.DefaultCommissionRate)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(10);
    }
}

