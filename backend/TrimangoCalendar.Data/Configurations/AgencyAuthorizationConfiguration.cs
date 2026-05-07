public class AgencyAuthorizationConfiguration : IEntityTypeConfiguration<AgencyAuthorization>
{
    public void Configure(EntityTypeBuilder<AgencyAuthorization> builder)
    {
        builder.HasKey(a => a.Id);
        
        // Bir acente aynı mülk için sadece bir yetki alabilir
        builder.HasIndex(a => new { a.AgencyId, a.PropertyId }).IsUnique();
        
        builder.HasOne(a => a.Agency)
            .WithMany(ag => ag.Authorizations)
            .HasForeignKey(a => a.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.Property)
            .WithMany()
            .HasForeignKey(a => a.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(a => a.CustomCommissionRate)
            .HasColumnType("decimal(5,2)");
        
        builder.Property(a => a.MaxMarkupRate)
            .HasColumnType("decimal(5,2)");
        
        builder.Property(a => a.DefaultMarkupRate)
            .HasColumnType("decimal(5,2)");
    }
}
