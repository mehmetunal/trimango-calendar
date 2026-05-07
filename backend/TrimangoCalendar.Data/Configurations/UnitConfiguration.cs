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
