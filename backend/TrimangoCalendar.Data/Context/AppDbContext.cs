// backend/TrimangoCalendar.Data/Context/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core;
using TrimangoCalendar.Core.Entities;

namespace TrimangoCalendar.Data.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ==========================================
        // TÜM DBSET TANIMLARI
        // ==========================================

        // Tenant & User
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantSettings> TenantSettings { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }

        // Property
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyReview> PropertyReviews { get; set; }

        // Unit
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitAmenity> UnitAmenities { get; set; }
        public DbSet<UnitImage> UnitImages { get; set; }

        // Reservation
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationHistory> ReservationHistories { get; set; }
        public DbSet<ReservationPayment> ReservationPayments { get; set; }

        // Guest
        public DbSet<Guest> Guests { get; set; }
        public DbSet<GuestDocument> GuestDocuments { get; set; }
        public DbSet<GuestNote> GuestNotes { get; set; }

        // Agency
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<AgencyUser> AgencyUsers { get; set; }
        public DbSet<AgencyAuthorization> AgencyAuthorizations { get; set; }
        public DbSet<AgencyCommission> AgencyCommissions { get; set; }

        // Pricing
        public DbSet<SeasonRate> SeasonRates { get; set; }
        public DbSet<SpecialDayRate> SpecialDayRates { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }

        // Currency
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<CurrencyFormat> CurrencyFormats { get; set; }

        // Calendar
        public DbSet<CalendarBlock> CalendarBlocks { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<CalendarNote> CalendarNotes { get; set; }

        // Notification
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        // Widget
        public DbSet<BookingWidget> BookingWidgets { get; set; }
        public DbSet<WidgetIntegration> WidgetIntegrations { get; set; }
        public DbSet<WidgetTheme> WidgetThemes { get; set; }

        // Report
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportSchedule> ReportSchedules { get; set; }
        public DbSet<DashboardWidget> DashboardWidgets { get; set; }

        // System
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<SmsTemplate> SmsTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // TENANT
            // ==========================================
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Subdomain).IsUnique();
                entity.HasIndex(e => e.Email);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.TaxNumber).HasMaxLength(20);
                entity.Property(e => e.TaxOffice).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Türkiye");
                entity.Property(e => e.Plan).HasMaxLength(20).HasDefaultValue("Free");
                entity.Property(e => e.MaxProperties).HasDefaultValue(5);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
            });

            // ==========================================
            // TENANT SETTINGS
            // ==========================================
            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId).IsUnique();

                entity.Property(e => e.Timezone).HasMaxLength(50).HasDefaultValue("Europe/Istanbul");
                entity.Property(e => e.DateFormat).HasMaxLength(20).HasDefaultValue("dd.MM.yyyy");
                entity.Property(e => e.DefaultCurrency).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.DefaultLanguage).HasMaxLength(5).HasDefaultValue("tr");

                entity.HasOne(e => e.Tenant)
                    .WithOne()
                    .HasForeignKey<TenantSettings>(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // SUBSCRIPTION PLANS
            // ==========================================
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.MaxProperties).HasDefaultValue(5);
                entity.Property(e => e.MaxUsers).HasDefaultValue(1);
                entity.Property(e => e.MaxAgencies).HasDefaultValue(5);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // ==========================================
            // TENANT SUBSCRIPTION
            // ==========================================
            modelBuilder.Entity<TenantSubscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });

                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Plan)
                    .WithMany()
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // PROPERTY
            // ==========================================
            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.City);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ShortDescription).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Website).HasMaxLength(500);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Türkiye");
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");
                entity.Property(e => e.Amenities).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Rules).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CancellationPolicy).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CoverImage).HasMaxLength(500);
                entity.Property(e => e.AverageRating).HasColumnType("decimal(3,2)").HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsFeatured).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Properties)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // PROPERTY IMAGE
            // ==========================================
            modelBuilder.Entity<PropertyImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ThumbnailPath).HasMaxLength(1000);
                entity.Property(e => e.ContentType).HasMaxLength(50);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.IsMain).HasDefaultValue(false);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // PROPERTY REVIEW
            // ==========================================
            modelBuilder.Entity<PropertyReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PropertyId, e.GuestId, e.ReservationId }).IsUnique();

                entity.Property(e => e.Comment).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Response).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Rating).HasDefaultValue(5);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // UNIT
            // ==========================================
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PropertyId, e.UnitNumber });

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitNumber).HasMaxLength(50);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SizeUnit).HasMaxLength(5).HasDefaultValue("m²");
                entity.Property(e => e.View).HasMaxLength(100);
                entity.Property(e => e.BedConfiguration).HasColumnType("nvarchar(max)");
                entity.Property(e => e.RoomAmenities).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ExtraBedPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxAdults).HasDefaultValue(2);
                entity.Property(e => e.MaxChildren).HasDefaultValue(0);
                entity.Property(e => e.MaxInfants).HasDefaultValue(0);
                entity.Property(e => e.TotalBeds).HasDefaultValue(1);
                entity.Property(e => e.ExtraBedCapacity).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany(p => p.Units)
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // RESERVATION
            // ==========================================
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReservationNumber).IsUnique();
                entity.HasIndex(e => new { e.UnitId, e.CheckIn, e.CheckOut });
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.GuestId);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.ReservationNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ServiceFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.SpecialRequests).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Notes).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                entity.Property(e => e.PromoCode).HasMaxLength(50);
                entity.Property(e => e.ExternalReference).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValue(ReservationStatus.Pending);
                entity.Property(e => e.Source).HasDefaultValue(ReservationSource.Direct);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

                entity.HasOne(e => e.Unit)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Guest)
                    .WithMany(g => g.Reservations)
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // RESERVATION HISTORY
            // ==========================================
            modelBuilder.Entity<ReservationHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Note).HasMaxLength(500);
                entity.Property(e => e.ChangedBy).HasMaxLength(100);
                entity.Property(e => e.ChangedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Reservation)
                    .WithMany(r => r.History)
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // RESERVATION PAYMENT
            // ==========================================
            modelBuilder.Entity<ReservationPayment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(200);
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.PaidAt).HasColumnType("datetime2");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Reservation)
                    .WithMany(r => r.Payments)
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // GUEST
            // ==========================================
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Email });
                entity.HasIndex(e => e.Phone);
                entity.HasIndex(e => e.TcKimlikNo);
                entity.HasIndex(e => e.PassportNumber);

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Phone2).HasMaxLength(20);
                entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
                entity.Property(e => e.PassportNumber).HasMaxLength(50);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.GuestType).HasMaxLength(20).HasDefaultValue("Regular");
                entity.Property(e => e.TotalSpent).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.PreferredLanguage).HasMaxLength(5).HasDefaultValue("tr");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
            });

            // ==========================================
            // GUEST DOCUMENT
            // ==========================================
            modelBuilder.Entity<GuestDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DocumentType).HasMaxLength(50);
                entity.Property(e => e.DocumentNumber).HasMaxLength(100);
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Guest)
                    .WithMany()
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // GUEST NOTE
            // ==========================================
            modelBuilder.Entity<GuestNote>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Note).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Guest)
                    .WithMany()
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // AGENCY
            // ==========================================
            modelBuilder.Entity<Agency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TaxNumber);

                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(300);
                entity.Property(e => e.TaxNumber).HasMaxLength(20);
                entity.Property(e => e.TaxOffice).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100).HasDefaultValue("Türkiye");
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Website).HasMaxLength(500);
                entity.Property(e => e.ContactPerson).HasMaxLength(200);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.ContactEmail).HasMaxLength(256);
                entity.Property(e => e.DefaultCommissionRate).HasColumnType("decimal(5,2)").HasDefaultValue(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsVerified).HasDefaultValue(false);
                entity.Property(e => e.Notes).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");
            });

            // ==========================================
            // AGENCY USER
            // ==========================================
            modelBuilder.Entity<AgencyUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AgencyId, e.Email }).IsUnique();

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Agency)
                    .WithMany(a => a.Users)
                    .HasForeignKey(e => e.AgencyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // AGENCY AUTHORIZATION
            // ==========================================
            modelBuilder.Entity<AgencyAuthorization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AgencyId, e.PropertyId }).IsUnique();

                entity.Property(e => e.AllowedUnitIds).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CustomCommissionRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MaxMarkupRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DefaultMarkupRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Notes).HasColumnType("nvarchar(max)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.GrantedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.RevokedAt).HasColumnType("datetime2");
                entity.Property(e => e.GrantedBy).HasMaxLength(100);
                entity.Property(e => e.PriceDisplay).HasDefaultValue(PriceDisplayType.Net);

                entity.HasOne(e => e.Agency)
                    .WithMany(a => a.Authorizations)
                    .HasForeignKey(e => e.AgencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Property)
                    .WithMany()
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // AGENCY COMMISSION
            // ==========================================
            modelBuilder.Entity<AgencyCommission>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CommissionRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3);
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.PaidAt).HasColumnType("datetime2");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Agency)
                    .WithMany()
                    .HasForeignKey(e => e.AgencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reservation)
                    .WithMany()
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // SEASON RATE
            // ==========================================
            modelBuilder.Entity<SeasonRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UnitId, e.StartDate, e.EndDate });

                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.WeekdayPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.WeekendPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SpecialDayPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.CancellationPolicy).HasMaxLength(50);
                entity.Property(e => e.CancellationFee).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MinStayDays).HasDefaultValue(1);
                entity.Property(e => e.MaxStayDays).HasDefaultValue(30);
                entity.Property(e => e.FreeCancellationDays).HasDefaultValue(7);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Unit)
                    .WithMany(u => u.SeasonRates)
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // SPECIAL DAY RATE
            // ==========================================
            modelBuilder.Entity<SpecialDayRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UnitId, e.Date }).IsUnique();

                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");

                entity.HasOne(e => e.Unit)
                    .WithMany()
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // PROMOTION
            // ==========================================
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasDefaultValue("TRY");
                entity.Property(e => e.MinStayDays).HasDefaultValue(1);
                entity.Property(e => e.MaxUsageCount).HasDefaultValue(100);
                entity.Property(e => e.UsedCount).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany()
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Unit)
                    .WithMany()
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==========================================
            // PROMOTION USAGE
            // ==========================================
            modelBuilder.Entity<PromotionUsage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PromotionId, e.ReservationId }).IsUnique();

                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UsedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Promotion)
                    .WithMany()
                    .HasForeignKey(e => e.PromotionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CURRENCY
            // ==========================================
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code).HasMaxLength(3);
                entity.Property(e => e.Symbol).HasMaxLength(5);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CultureCode).HasMaxLength(10);
                entity.Property(e => e.DecimalPlaces).HasDefaultValue(2);
                entity.Property(e => e.IsBaseCurrency).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // EXCHANGE RATE
            // ==========================================
            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.BaseCurrencyCode, e.TargetCurrencyCode, e.Date }).IsUnique();

                entity.Property(e => e.Rate).HasColumnType("decimal(18,6)");
                entity.Property(e => e.BuyRate).HasColumnType("decimal(18,6)");
                entity.Property(e => e.SellRate).HasColumnType("decimal(18,6)");
                entity.Property(e => e.BaseCurrencyCode).HasMaxLength(3);
                entity.Property(e => e.TargetCurrencyCode).HasMaxLength(3);
                entity.Property(e => e.Source).HasMaxLength(50).HasDefaultValue("TCMB");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.BaseCurrency)
                    .WithMany(c => c.BaseRates)
                    .HasForeignKey(e => e.BaseCurrencyCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TargetCurrency)
                    .WithMany(c => c.TargetRates)
                    .HasForeignKey(e => e.TargetCurrencyCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CALENDAR BLOCK
            // ==========================================
            modelBuilder.Entity<CalendarBlock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UnitId, e.StartDate, e.EndDate });

                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasColumnType("nvarchar(max)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Unit)
                    .WithMany()
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // NOTIFICATION
            // ==========================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });

                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Channel).HasMaxLength(20);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Message).HasColumnType("nvarchar(max)");
                entity.Property(e => e.TemplateCode).HasMaxLength(50);
                entity.Property(e => e.RecipientEmail).HasMaxLength(256);
                entity.Property(e => e.RecipientPhone).HasMaxLength(20);
                entity.Property(e => e.ReferenceType).HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValue(NotificationStatus.Pending);
                entity.Property(e => e.ErrorMessage).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.SentAt).HasColumnType("datetime2");
                entity.Property(e => e.ReadAt).HasColumnType("datetime2");
            });

            // ==========================================
            // NOTIFICATION TEMPLATE
            // ==========================================
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();

                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subject).HasMaxLength(500);
                entity.Property(e => e.BodyTemplate).HasColumnType("nvarchar(max)");
                entity.Property(e => e.SMSTemplate).HasColumnType("nvarchar(max)");
                entity.Property(e => e.AvailableVariables).HasColumnType("nvarchar(max)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // NOTIFICATION PREFERENCE
            // ==========================================
            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Type }).IsUnique();

                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EmailAddresses).HasColumnType("nvarchar(max)");
                entity.Property(e => e.PhoneNumbers).HasColumnType("nvarchar(max)");
                entity.Property(e => e.EmailEnabled).HasDefaultValue(true);
                entity.Property(e => e.SMSEnabled).HasDefaultValue(false);
                entity.Property(e => e.InAppEnabled).HasDefaultValue(true);
                entity.Property(e => e.PushEnabled).HasDefaultValue(false);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // BOOKING WIDGET
            // ==========================================
            modelBuilder.Entity<BookingWidget>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WidgetKey).IsUnique();

                entity.Property(e => e.WidgetKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Theme).HasMaxLength(50).HasDefaultValue("default");
                entity.Property(e => e.PrimaryColor).HasMaxLength(7).HasDefaultValue("#2563EB");
                entity.Property(e => e.SecondaryColor).HasMaxLength(7).HasDefaultValue("#1E40AF");
                entity.Property(e => e.FontFamily).HasMaxLength(100).HasDefaultValue("Inter, sans-serif");
                entity.Property(e => e.CustomCSS).HasColumnType("nvarchar(max)");
                entity.Property(e => e.MetaTitle).HasMaxLength(200);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);
                entity.Property(e => e.SharingImage).HasMaxLength(500);
                entity.Property(e => e.DefaultLanguage).HasMaxLength(5).HasDefaultValue("tr");
                entity.Property(e => e.AvailableLanguages).HasMaxLength(500).HasDefaultValue("[\"tr\",\"en\"]");
                entity.Property(e => e.Position).HasDefaultValue(WidgetPosition.Left);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany()
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // WIDGET INTEGRATION
            // ==========================================
            modelBuilder.Entity<WidgetIntegration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.WidgetId, e.Domain }).IsUnique();

                entity.Property(e => e.Domain).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Widget)
                    .WithMany(w => w.Integrations)
                    .HasForeignKey(e => e.WidgetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // REPORT
            // ==========================================
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.FileFormat).HasMaxLength(10);
                entity.Property(e => e.Filters).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Status).HasDefaultValue(ReportStatus.Pending);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // AUDIT LOG
            // ==========================================
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });

                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
                entity.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ==========================================
            // SYSTEM SETTINGS (Seed Data)
            // ==========================================
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Code = "TRY", Symbol = "₺", Name = "Türk Lirası", CultureCode = "tr-TR", IsBaseCurrency = true, IsActive = true },
                new Currency { Code = "USD", Symbol = "$", Name = "Amerikan Doları", CultureCode = "en-US", IsActive = true },
                new Currency { Code = "EUR", Symbol = "€", Name = "Euro", CultureCode = "de-DE", IsActive = true },
                new Currency { Code = "GBP", Symbol = "£", Name = "İngiliz Sterlini", CultureCode = "en-GB", IsActive = true }
            );

            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Free", Code = "free", Price = 0, MaxProperties = 5, MaxUsers = 1, MaxAgencies = 3, IsActive = true },
                new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Pro", Code = "pro", Price = 49, MaxProperties = 25, MaxUsers = 5, MaxAgencies = 10, IsActive = true },
                new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Enterprise", Code = "enterprise", Price = 199, MaxProperties = 999, MaxUsers = 999, MaxAgencies = 999, IsActive = true }
            );
        }
    }
}
