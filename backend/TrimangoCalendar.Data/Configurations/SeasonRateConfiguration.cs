public class SeasonRateConfiguration : IEntityTypeConfiguration<SeasonRate>
{
    public void Configure(EntityTypeBuilder<SeasonRate> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.HasIndex(s => new { s.UnitId, s.StartDate, s.EndDate });
        
        builder.Property(s => s.WeekdayPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.Property(s => s.WeekendPrice)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(s => s.SpecialDayPrice)
            .HasColumnType("decimal(18,2)");
        
        builder.HasOne(s => s.Unit)
            .WithMany(u => u.SeasonRates)
            .HasForeignKey(s => s.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
