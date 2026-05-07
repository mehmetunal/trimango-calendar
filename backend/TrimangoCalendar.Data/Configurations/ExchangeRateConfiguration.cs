public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.BaseCurrencyCode, e.TargetCurrencyCode, e.Date })
            .IsUnique();
        
        builder.Property(e => e.Rate)
            .HasColumnType("decimal(18,6)");
        
        builder.Property(e => e.BuyRate)
            .HasColumnType("decimal(18,6)");
        
        builder.Property(e => e.SellRate)
            .HasColumnType("decimal(18,6)");
        
        builder.HasOne(e => e.BaseCurrency)
            .WithMany(c => c.BaseRates)
            .HasForeignKey(e => e.BaseCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.TargetCurrency)
            .WithMany(c => c.TargetRates)
            .HasForeignKey(e => e.TargetCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

