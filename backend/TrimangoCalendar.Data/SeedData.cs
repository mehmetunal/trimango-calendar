using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrimangoCalendar.Core;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;

namespace TrimangoCalendar.Data;

public static class SeedData
{
    private const int MinRecordCount = 150;

    public static Task InitializeAsync(IServiceProvider serviceProvider) => Initialize(serviceProvider);

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedIdentity(roleManager, userManager, context);
        await SeedCurrencies(context);
        await SeedDomainData(context);
        await SeedExchangeRates(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedIdentity(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        var roles = new[] { "Admin", "TenantOwner", "AgencyUser" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        if (!await context.Tenants.AnyAsync(t => t.Subdomain == "demo"))
        {
            context.Tenants.Add(new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Trimango Demo",
                Subdomain = "demo",
                Email = "admin@trimango.local",
                Phone = "5550000000",
                Address = "Demo Mahallesi No:1",
                City = "Istanbul",
                Country = "Türkiye",
                TaxNumber = "1111111111",
                TaxOffice = "Demo Vergi Dairesi",
                IsActive = true,
                Plan = "Pro",
                PlanStartDate = DateTime.UtcNow,
                MaxProperties = 25,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        var tenant = await context.Tenants.FirstAsync(t => t.Subdomain == "demo");
        if (await userManager.FindByEmailAsync("admin@trimango.local") is null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@trimango.local",
                EmailConfirmed = true,
                TenantId = tenant.Id,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true
            };

            var create = await userManager.CreateAsync(user, "Admin123!");
            if (create.Succeeded)
            {
                await userManager.AddToRolesAsync(user, roles);
            }
        }
    }

    private static async Task SeedCurrencies(AppDbContext context)
    {
        var existingCodes = await context.Currencies.Select(x => x.Code).ToListAsync();
        var now = DateTime.UtcNow;

        if (!existingCodes.Contains("TRY"))
        {
            context.Currencies.Add(new Currency { Code = "TRY", Symbol = "₺", Name = "Türk Lirası", CultureCode = "tr-TR", IsBaseCurrency = true, IsActive = true, CreatedAt = now });
        }

        if (!existingCodes.Contains("USD"))
        {
            context.Currencies.Add(new Currency { Code = "USD", Symbol = "$", Name = "US Dollar", CultureCode = "en-US", IsActive = true, CreatedAt = now });
        }

        if (!existingCodes.Contains("EUR"))
        {
            context.Currencies.Add(new Currency { Code = "EUR", Symbol = "€", Name = "Euro", CultureCode = "de-DE", IsActive = true, CreatedAt = now });
        }

        if (!existingCodes.Contains("GBP"))
        {
            context.Currencies.Add(new Currency { Code = "GBP", Symbol = "£", Name = "Pound Sterling", CultureCode = "en-GB", IsActive = true, CreatedAt = now });
        }

        var missing = MinRecordCount - await context.Currencies.CountAsync();
        if (missing <= 0)
        {
            return;
        }

        var faker = new Faker("tr");
        var generated = new List<Currency>();
        var allCodes = new HashSet<string>(await context.Currencies.Select(x => x.Code).ToListAsync());

        var idx = 0;
        while (generated.Count < missing)
        {
            var code = BuildThreeLetterCurrencyCode(idx);
            idx++;
            if (!allCodes.Add(code))
            {
                continue;
            }

            generated.Add(new Currency
            {
                Code = code,
                Symbol = faker.PickRandom("$", "€", "₺", "£"),
                Name = $"Seed Currency {code}",
                CultureCode = "tr-TR",
                IsActive = true,
                CreatedAt = now
            });
        }

        context.Currencies.AddRange(generated);
    }

    private static string BuildThreeLetterCurrencyCode(int index)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var first = letters[(index / (26 * 26)) % 26];
        var second = letters[(index / 26) % 26];
        var third = letters[index % 26];
        return $"{first}{second}{third}";
    }

    private static async Task SeedDomainData(AppDbContext context)
    {
        var now = DateTime.UtcNow;
        var faker = new Faker("tr");

        var tenantMissing = MinRecordCount - await context.Tenants.CountAsync();
        if (tenantMissing > 0)
        {
            var tenantFaker = new Faker<Tenant>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.Name, f => $"{f.Company.CompanyName()} Otelcilik")
                .RuleFor(x => x.Subdomain, f => $"t{f.Random.AlphaNumeric(8).ToLowerInvariant()}")
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Phone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
                .RuleFor(x => x.Address, f => f.Address.StreetAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.Country, _ => "Türkiye")
                .RuleFor(x => x.TaxNumber, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(x => x.TaxOffice, f => $"{f.Address.City()} Vergi Dairesi")
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.Plan, _ => "Pro")
                .RuleFor(x => x.PlanStartDate, _ => now)
                .RuleFor(x => x.MaxProperties, _ => 250)
                .RuleFor(x => x.CreatedAt, _ => now);

            context.Tenants.AddRange(tenantFaker.Generate(tenantMissing));
            await context.SaveChangesAsync();
        }

        var tenants = await context.Tenants.ToListAsync();
        var tenantIds = tenants.Select(x => x.Id).ToList();

        var planMissing = MinRecordCount - await context.SubscriptionPlans.CountAsync();
        if (planMissing > 0)
        {
            var plans = new Faker<SubscriptionPlan>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.Name, f => $"{f.Commerce.ProductAdjective()} Plan")
                .RuleFor(x => x.Code, f => $"plan-{f.Random.AlphaNumeric(10).ToLowerInvariant()}")
                .RuleFor(x => x.Price, f => f.Random.Decimal(0, 9999))
                .RuleFor(x => x.CurrencyCode, _ => "TRY")
                .RuleFor(x => x.MaxProperties, f => f.Random.Int(5, 500))
                .RuleFor(x => x.MaxUsers, f => f.Random.Int(1, 300))
                .RuleFor(x => x.MaxAgencies, f => f.Random.Int(1, 300))
                .RuleFor(x => x.IsActive, _ => true)
                .Generate(planMissing);

            context.SubscriptionPlans.AddRange(plans);
            await context.SaveChangesAsync();
        }

        var plansList = await context.SubscriptionPlans.ToListAsync();
        var tenantSettingsMissing = MinRecordCount - await context.TenantSettings.CountAsync();
        if (tenantSettingsMissing > 0)
        {
            var usedTenantSettings = await context.TenantSettings.Select(x => x.TenantId).ToListAsync();
            var candidateTenantIds = tenantIds.Except(usedTenantSettings).Take(tenantSettingsMissing).ToList();
            context.TenantSettings.AddRange(candidateTenantIds.Select(tid => new TenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tid,
                Timezone = "Europe/Istanbul",
                DateFormat = "dd.MM.yyyy",
                DefaultCurrency = "TRY",
                DefaultLanguage = "tr"
            }));
            await context.SaveChangesAsync();
        }

        var tenantSubscriptionMissing = MinRecordCount - await context.TenantSubscriptions.CountAsync();
        if (tenantSubscriptionMissing > 0)
        {
            var subscriptionUsedTenantIds = await context.TenantSubscriptions.Select(x => x.TenantId).ToListAsync();
            var subscriptionTenantIds = tenantIds.Except(subscriptionUsedTenantIds).Take(tenantSubscriptionMissing).ToList();
            context.TenantSubscriptions.AddRange(subscriptionTenantIds.Select(tid => new TenantSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = tid,
                PlanId = faker.PickRandom(plansList).Id,
                Amount = faker.Random.Decimal(0, 3000),
                CurrencyCode = "TRY",
                Status = faker.PickRandom("Active", "Pending", "Expired"),
                PaymentMethod = faker.PickRandom("CreditCard", "Transfer", "Cash")
            }));
            await context.SaveChangesAsync();
        }

        var propertyMissing = MinRecordCount - await context.Properties.CountAsync();
        var propertiesWithMissingCoverImage = await context.Properties
            .Where(x => string.IsNullOrWhiteSpace(x.CoverImage))
            .ToListAsync();
        if (propertiesWithMissingCoverImage.Count > 0)
        {
            foreach (var property in propertiesWithMissingCoverImage)
            {
                property.CoverImage = $"/uploads/properties/{property.Id}/cover_default.jpg";
            }

            await context.SaveChangesAsync();
        }

        if (propertyMissing > 0)
        {
            var properties = new Faker<Property>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.TenantId, f => f.PickRandom(tenantIds))
                .RuleFor(x => x.Type, f => f.PickRandom<PropertyType>())
                .RuleFor(x => x.Name, f => $"{f.Address.City()} Residence {f.Random.Int(100, 999)}")
                .RuleFor(x => x.Slug, f => $"property-{f.Random.AlphaNumeric(12).ToLowerInvariant()}")
                .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
                .RuleFor(x => x.ShortDescription, f => f.Lorem.Sentence(6))
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Phone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
                .RuleFor(x => x.Website, f => f.Internet.Url())
                .RuleFor(x => x.Address, f => f.Address.StreetAddress())
                .RuleFor(x => x.District, f => f.Address.County())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.Country, _ => "Türkiye")
                .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
                .RuleFor(x => x.Amenities, _ => "[]")
                .RuleFor(x => x.Rules, _ => "[]")
                .RuleFor(x => x.CancellationPolicy, _ => "Esnek")
                .RuleFor(x => x.CoverImage, (f, x) => $"/uploads/properties/{x.Id}/cover_{f.Random.Int(1000, 9999)}.jpg")
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(propertyMissing);

            context.Properties.AddRange(properties);
            await context.SaveChangesAsync();
        }

        var propertiesList = await context.Properties.ToListAsync();
        var propertyIds = propertiesList.Select(x => x.Id).ToList();

        var unitMissing = MinRecordCount - await context.Units.CountAsync();
        if (unitMissing > 0)
        {
            var units = new Faker<Unit>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.PropertyId, f => f.PickRandom(propertyIds))
                .RuleFor(x => x.Name, f => $"{f.Commerce.ProductAdjective()} Oda")
                .RuleFor(x => x.UnitNumber, f => $"{f.Random.Int(1, 90)}{f.Random.Char('A', 'Z')}")
                .RuleFor(x => x.Description, f => f.Lorem.Sentence())
                .RuleFor(x => x.Floor, f => f.Random.Int(0, 10))
                .RuleFor(x => x.BasePrice, f => f.Random.Decimal(1000, 9000))
                .RuleFor(x => x.CurrencyCode, _ => "TRY")
                .RuleFor(x => x.MaxAdults, f => f.Random.Int(1, 5))
                .RuleFor(x => x.MaxChildren, f => f.Random.Int(0, 3))
                .RuleFor(x => x.MaxInfants, f => f.Random.Int(0, 2))
                .RuleFor(x => x.TotalBeds, f => f.Random.Int(1, 4))
                .RuleFor(x => x.BedConfiguration, _ => "[]")
                .RuleFor(x => x.View, f => f.PickRandom("Deniz", "Şehir", "Dağ", "Bahçe"))
                .RuleFor(x => x.RoomAmenities, _ => "[]")
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(unitMissing);

            context.Units.AddRange(units);
            await context.SaveChangesAsync();
        }

        var unitsList = await context.Units.ToListAsync();
        var unitIds = unitsList.Select(x => x.Id).ToList();
        var unitById = unitsList.ToDictionary(x => x.Id);

        var guestMissing = MinRecordCount - await context.Guests.CountAsync();
        if (guestMissing > 0)
        {
            var guests = new Faker<Guest>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.TenantId, f => f.PickRandom(tenantIds))
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.Email, (f, x) => f.Internet.Email(x.FirstName, x.LastName))
                .RuleFor(x => x.Phone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
                .RuleFor(x => x.Phone2, _ => "")
                .RuleFor(x => x.TcKimlikNo, f => f.Random.ReplaceNumbers("###########"))
                .RuleFor(x => x.PassportNumber, _ => "")
                .RuleFor(x => x.Nationality, _ => "Türkiye")
                .RuleFor(x => x.Gender, f => f.PickRandom("M", "F"))
                .RuleFor(x => x.Address, f => f.Address.StreetAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.Country, _ => "Türkiye")
                .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
                .RuleFor(x => x.GuestType, _ => "Regular")
                .RuleFor(x => x.TotalStays, f => f.Random.Int(0, 15))
                .RuleFor(x => x.TotalNights, f => f.Random.Int(0, 120))
                .RuleFor(x => x.TotalSpent, f => f.Random.Decimal(0, 150000))
                .RuleFor(x => x.AllowMarketing, f => f.Random.Bool())
                .RuleFor(x => x.AllowSms, f => f.Random.Bool())
                .RuleFor(x => x.PreferredLanguage, _ => "tr")
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(guestMissing);

            context.Guests.AddRange(guests);
            await context.SaveChangesAsync();
        }

        var guestsList = await context.Guests.ToListAsync();
        var guestIds = guestsList.Select(x => x.Id).ToList();
        var guestById = guestsList.ToDictionary(x => x.Id);

        var reservationsWithMissingRequiredText = await context.Reservations
            .Where(x =>
                string.IsNullOrWhiteSpace(x.StatusNote) ||
                string.IsNullOrWhiteSpace(x.PromoCode) ||
                string.IsNullOrWhiteSpace(x.SpecialRequests) ||
                string.IsNullOrWhiteSpace(x.Notes) ||
                string.IsNullOrWhiteSpace(x.CancellationReason) ||
                string.IsNullOrWhiteSpace(x.ExternalReference) ||
                string.IsNullOrWhiteSpace(x.CreatedBy))
            .ToListAsync();
        if (reservationsWithMissingRequiredText.Count > 0)
        {
            foreach (var reservation in reservationsWithMissingRequiredText)
            {
                reservation.StatusNote ??= "Durum güncellemesi yok";
                reservation.PromoCode ??= "";
                reservation.SpecialRequests ??= "";
                reservation.Notes ??= "";
                reservation.CancellationReason ??= "";
                reservation.ExternalReference ??= "";
                reservation.CreatedBy ??= "seed";
            }

            await context.SaveChangesAsync();
        }

        var reservationMissing = MinRecordCount - await context.Reservations.CountAsync();
        if (reservationMissing > 0)
        {
            var reservations = new List<Reservation>();
            for (var i = 0; i < reservationMissing; i++)
            {
                var unit = faker.PickRandom(unitsList);
                var guest = faker.PickRandom(guestsList);
                var checkIn = now.Date.AddDays(faker.Random.Int(-30, 240));
                var checkOut = checkIn.AddDays(faker.Random.Int(1, 14));
                var total = faker.Random.Decimal(2500, 35000);
                var paid = Math.Round(total * faker.Random.Decimal(0.1m, 1m), 2);

                reservations.Add(new Reservation
                {
                    Id = Guid.NewGuid(),
                    TenantId = unitById[unit.Id].PropertyId != Guid.Empty
                        ? propertiesList.First(p => p.Id == unit.PropertyId).TenantId
                        : faker.PickRandom(tenantIds),
                    UnitId = unit.Id,
                    GuestId = guest.Id,
                    ReservationNumber = $"R{DateTime.UtcNow:yyMMdd}{i:0000}{faker.Random.Int(10, 99)}",
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    TotalNights = (checkOut - checkIn).Days,
                    Adults = faker.Random.Int(1, 5),
                    Children = faker.Random.Int(0, 3),
                    Infants = faker.Random.Int(0, 1),
                    CurrencyCode = "TRY",
                    TotalAmount = total,
                    PaidAmount = paid,
                    RemainingAmount = total - paid,
                    Status = faker.PickRandom<ReservationStatus>(),
                    StatusNote = "Seed kaydı oluşturuldu",
                    StatusChangedAt = now,
                    Source = faker.PickRandom<ReservationSource>(),
                    TaxAmount = Math.Round(total * 0.10m, 2),
                    ServiceFee = Math.Round(total * 0.05m, 2),
                    DiscountAmount = faker.Random.Bool() ? Math.Round(total * 0.03m, 2) : 0m,
                    PromoCode = "",
                    SpecialRequests = "",
                    Notes = "",
                    IsLateCheckout = false,
                    CancellationReason = "",
                    RefundAmount = 0m,
                    ExternalReference = "",
                    CreatedBy = "seed",
                    CreatedAt = now
                });
            }

            context.Reservations.AddRange(reservations);
            await context.SaveChangesAsync();
        }

        var reservationsList = await context.Reservations.ToListAsync();
        var reservationIds = reservationsList.Select(x => x.Id).ToList();

        var propertyImageMissing = MinRecordCount - await context.PropertyImages.CountAsync();
        if (propertyImageMissing > 0)
        {
            var images = new Faker<PropertyImage>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.PropertyId, f => f.PickRandom(propertyIds))
                .RuleFor(x => x.FileName, f => $"property_{f.Random.Int(100000, 999999)}.jpg")
                .RuleFor(x => x.OriginalFileName, (f, x) => x.FileName)
                .RuleFor(x => x.FilePath, (f, x) => $"/uploads/properties/{x.PropertyId}/{x.FileName}")
                .RuleFor(x => x.ThumbnailPath, (f, x) => $"/uploads/properties/{x.PropertyId}/thumb_{x.FileName}")
                .RuleFor(x => x.FileSize, f => f.Random.Long(100000, 4000000))
                .RuleFor(x => x.ContentType, _ => "image/jpeg")
                .RuleFor(x => x.SortOrder, f => f.Random.Int(1, 10))
                .RuleFor(x => x.IsMain, f => f.Random.Bool(0.2f))
                .RuleFor(x => x.UploadedAt, _ => now)
                .Generate(propertyImageMissing);

            context.PropertyImages.AddRange(images);
            await context.SaveChangesAsync();
        }

        var propertyReviewMissing = MinRecordCount - await context.PropertyReviews.CountAsync();
        if (propertyReviewMissing > 0)
        {
            var usedReviewPairs = await context.PropertyReviews
                .Select(x => new { x.PropertyId, x.GuestId, x.ReservationId })
                .ToListAsync();
            var used = new HashSet<string>(usedReviewPairs.Select(x => $"{x.PropertyId}:{x.GuestId}:{x.ReservationId}"));
            var reviews = new List<PropertyReview>();
            foreach (var reservation in reservationsList.OrderBy(_ => Guid.NewGuid()))
            {
                if (reviews.Count >= propertyReviewMissing)
                {
                    break;
                }

                var propertyId = unitById[reservation.UnitId].PropertyId;
                var key = $"{propertyId}:{reservation.GuestId}:{reservation.Id}";
                if (!used.Add(key))
                {
                    continue;
                }

                reviews.Add(new PropertyReview
                {
                    Id = Guid.NewGuid(),
                    PropertyId = propertyId,
                    GuestId = reservation.GuestId,
                    ReservationId = reservation.Id,
                    Rating = faker.Random.Int(3, 5),
                    CleanlinessRating = faker.Random.Int(3, 5),
                    ComfortRating = faker.Random.Int(3, 5),
                    LocationRating = faker.Random.Int(3, 5),
                    StaffRating = faker.Random.Int(3, 5),
                    Comment = faker.Lorem.Sentence(),
                    Response = faker.Lorem.Sentence(),
                    ResponseDate = now,
                    IsApproved = true,
                    CreatedAt = now
                });
            }

            context.PropertyReviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

        await EnsureSimpleMinCount(context.UnitAmenities, MinRecordCount, () => new UnitAmenity
        {
            Id = Guid.NewGuid(),
            UnitId = faker.PickRandom(unitIds),
            Name = faker.PickRandom("WiFi", "Klima", "TV", "Minibar", "Balkon")
        });

        await EnsureSimpleMinCount(context.UnitImages, MinRecordCount, () => new UnitImage
        {
            Id = Guid.NewGuid(),
            UnitId = faker.PickRandom(unitIds),
            Url = $"/uploads/units/{faker.PickRandom(unitIds)}/unit_{faker.Random.Int(100000, 999999)}.jpg"
        });

        await EnsureSimpleMinCount(context.ReservationHistories, MinRecordCount, () =>
        {
            var reservation = faker.PickRandom(reservationsList);
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = reservation.Id,
                OldStatus = ReservationStatus.Pending,
                NewStatus = reservation.Status,
                Note = "Seed geçmiş kaydı",
                ChangedBy = "seed",
                ChangedAt = now
            };
        });

        await EnsureSimpleMinCount(context.ReservationPayments, MinRecordCount, () =>
        {
            var reservation = faker.PickRandom(reservationsList);
            return new ReservationPayment
            {
                Id = Guid.NewGuid(),
                ReservationId = reservation.Id,
                Amount = faker.Random.Decimal(500, 5000),
                CurrencyCode = "TRY",
                PaymentMethod = faker.PickRandom("CreditCard", "Transfer", "Cash"),
                TransactionId = Guid.NewGuid().ToString("N"),
                Status = faker.PickRandom("Completed", "Pending", "Failed"),
                PaidAt = now.AddDays(-faker.Random.Int(0, 60)),
                CreatedAt = now
            };
        });

        await EnsureSimpleMinCount(context.GuestDocuments, MinRecordCount, () =>
        {
            var guest = faker.PickRandom(guestsList);
            return new GuestDocument
            {
                Id = Guid.NewGuid(),
                GuestId = guest.Id,
                DocumentType = faker.PickRandom("Passport", "IdentityCard"),
                DocumentNumber = faker.Random.ReplaceNumbers("###########"),
                FilePath = $"/uploads/guests/{guest.Id}/doc_{faker.Random.Int(10000, 99999)}.pdf",
                UploadedAt = now
            };
        });

        await EnsureSimpleMinCount(context.GuestNotes, MinRecordCount, () => new GuestNote
        {
            Id = Guid.NewGuid(),
            GuestId = faker.PickRandom(guestIds),
            Note = faker.Lorem.Sentence(),
            CreatedBy = "seed",
            CreatedAt = now
        });

        var agencyMissing = MinRecordCount - await context.Agencies.CountAsync();
        if (agencyMissing > 0)
        {
            var agencies = new Faker<Agency>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.TenantId, f => f.PickRandom(tenantIds))
                .RuleFor(x => x.CompanyName, f => f.Company.CompanyName())
                .RuleFor(x => x.TaxNumber, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(x => x.TaxOffice, f => $"{f.Address.City()} Vergi Dairesi")
                .RuleFor(x => x.Address, f => f.Address.StreetAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.Country, _ => "Türkiye")
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Phone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
                .RuleFor(x => x.Website, f => f.Internet.Url())
                .RuleFor(x => x.ContactPerson, f => f.Name.FullName())
                .RuleFor(x => x.ContactPhone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
                .RuleFor(x => x.ContactEmail, f => f.Internet.Email())
                .RuleFor(x => x.Type, f => f.PickRandom<AgencyType>())
                .RuleFor(x => x.DefaultCommissionRate, f => f.Random.Decimal(5, 25))
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.IsVerified, f => f.Random.Bool())
                .RuleFor(x => x.Notes, f => f.Lorem.Sentence())
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(agencyMissing);

            context.Agencies.AddRange(agencies);
            await context.SaveChangesAsync();
        }

        var agenciesList = await context.Agencies.ToListAsync();
        var agencyIds = agenciesList.Select(x => x.Id).ToList();
        var identityUsers = await context.Users.ToListAsync();
        if (identityUsers.Count == 0)
        {
            return;
        }

        var agencyUsersWithMissingUserId = await context.AgencyUsers
            .Where(x => string.IsNullOrWhiteSpace(x.UserId))
            .ToListAsync();
        if (agencyUsersWithMissingUserId.Count > 0)
        {
            foreach (var agencyUser in agencyUsersWithMissingUserId)
            {
                agencyUser.UserId = Guid.NewGuid().ToString();
            }

            await context.SaveChangesAsync();
        }

        var agencyUsersWithInvalidFk = await context.AgencyUsers
            .Where(x => !context.Users.Any(u => u.Id == EF.Property<Guid>(x, "UserId1")))
            .ToListAsync();
        if (agencyUsersWithInvalidFk.Count > 0)
        {
            foreach (var agencyUser in agencyUsersWithInvalidFk)
            {
                var identityUser = faker.PickRandom(identityUsers);
                agencyUser.UserId = identityUser.Id.ToString();
                context.Entry(agencyUser).Property("UserId1").CurrentValue = identityUser.Id;
            }

            await context.SaveChangesAsync();
        }

        var agencyUserMissing = MinRecordCount - await context.AgencyUsers.CountAsync();
        if (agencyUserMissing > 0)
        {
            var newAgencyUsers = new List<AgencyUser>();
            for (var i = 0; i < agencyUserMissing; i++)
            {
                var agency = faker.PickRandom(agenciesList);
                var identityUser = faker.PickRandom(identityUsers);
                var agencyUser = new AgencyUser
                {
                    Id = Guid.NewGuid(),
                    AgencyId = agency.Id,
                    UserId = identityUser.Id.ToString(),
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    Email = $"{Guid.NewGuid():N}@agency.local",
                    Phone = $"+90{faker.Random.ReplaceNumbers("5#########")}",
                    IsActive = true,
                    CreatedAt = now
                };

                context.Entry(agencyUser).Property("UserId1").CurrentValue = identityUser.Id;
                newAgencyUsers.Add(agencyUser);
            }

            context.AgencyUsers.AddRange(newAgencyUsers);
            await context.SaveChangesAsync();
        }

        var authorizationMissing = MinRecordCount - await context.AgencyAuthorizations.CountAsync();
        if (authorizationMissing > 0)
        {
            var existingPairs = await context.AgencyAuthorizations
                .Select(x => new { x.AgencyId, x.PropertyId })
                .ToListAsync();
            var usedAuth = new HashSet<string>(existingPairs.Select(x => $"{x.AgencyId}:{x.PropertyId}"));
            var authorizations = new List<AgencyAuthorization>();

            foreach (var agency in agenciesList.OrderBy(_ => Guid.NewGuid()))
            {
                if (authorizations.Count >= authorizationMissing)
                {
                    break;
                }

                var property = faker.PickRandom(propertiesList);
                var key = $"{agency.Id}:{property.Id}";
                if (!usedAuth.Add(key))
                {
                    continue;
                }

                authorizations.Add(new AgencyAuthorization
                {
                    Id = Guid.NewGuid(),
                    AgencyId = agency.Id,
                    PropertyId = property.Id,
                    GrantedByTenantId = property.TenantId,
                    Level = faker.PickRandom<AuthorizationLevel>(),
                    AllowedUnitIds = "[]",
                    CanViewPrices = true,
                    CanSetPrices = faker.Random.Bool(),
                    CanCreateReservation = true,
                    CanModifyReservation = faker.Random.Bool(),
                    CanCancelReservation = faker.Random.Bool(),
                    PriceDisplay = faker.PickRandom<PriceDisplayType>(),
                    CustomCommissionRate = faker.Random.Decimal(5, 20),
                    MaxMarkupRate = faker.Random.Decimal(10, 30),
                    DefaultMarkupRate = faker.Random.Decimal(3, 10),
                    ValidFrom = now.AddDays(-30),
                    ValidTo = now.AddDays(365),
                    HasAllotment = faker.Random.Bool(),
                    TotalAllotment = faker.Random.Int(5, 200),
                    UsedAllotment = faker.Random.Int(0, 20),
                    IsActive = true,
                    Notes = faker.Lorem.Sentence(),
                    GrantedAt = now,
                    GrantedBy = "seed"
                });
            }

            context.AgencyAuthorizations.AddRange(authorizations);
            await context.SaveChangesAsync();
        }

        await EnsureSimpleMinCount(context.AgencyCommissions, MinRecordCount, () =>
        {
            var reservation = faker.PickRandom(reservationsList);
            var propertyId = unitById[reservation.UnitId].PropertyId;
            return new AgencyCommission
            {
                Id = Guid.NewGuid(),
                AgencyId = faker.PickRandom(agencyIds),
                PropertyId = propertyId,
                ReservationId = reservation.Id,
                Amount = faker.Random.Decimal(200, 5000),
                CommissionRate = faker.Random.Decimal(5, 25),
                CurrencyCode = "TRY",
                Status = faker.PickRandom("Pending", "Paid"),
                PaidAt = faker.Random.Bool() ? now : null,
                MinCommissionRate = 5,
                MaxCommissionRate = 25,
                ValidFrom = now.AddMonths(-3),
                ValidTo = now.AddMonths(3),
                IsActive = true,
                CreatedAt = now
            };
        });

        var seasonRatesWithMissingPolicy = await context.SeasonRates
            .Where(x => string.IsNullOrWhiteSpace(x.CancellationPolicy))
            .ToListAsync();
        if (seasonRatesWithMissingPolicy.Count > 0)
        {
            foreach (var seasonRate in seasonRatesWithMissingPolicy)
            {
                seasonRate.CancellationPolicy = "Flexible";
            }

            await context.SaveChangesAsync();
        }

        await EnsureSimpleMinCount(context.SeasonRates, MinRecordCount, () =>
        {
            var start = now.Date.AddDays(faker.Random.Int(-30, 240));
            return new SeasonRate
            {
                Id = Guid.NewGuid(),
                UnitId = faker.PickRandom(unitIds),
                Name = faker.PickRandom("Yaz", "Kış", "Bayram", "Erken Rezervasyon"),
                StartDate = start,
                EndDate = start.AddDays(faker.Random.Int(5, 45)),
                WeekdayPrice = faker.Random.Decimal(1000, 7000),
                WeekendPrice = faker.Random.Decimal(1200, 9000),
                SpecialDayPrice = faker.Random.Decimal(1400, 11000),
                CurrencyCode = "TRY",
                MinStayDays = faker.Random.Int(1, 5),
                MaxStayDays = faker.Random.Int(6, 30),
                CancellationPolicy = faker.PickRandom("Flexible", "Moderate", "Strict"),
                FreeCancellationDays = faker.Random.Int(1, 14),
                CancellationFee = faker.Random.Decimal(0, 20),
                IsActive = true,
                CreatedAt = now
            };
        });

        var specialMissing = MinRecordCount - await context.SpecialDayRates.CountAsync();
        if (specialMissing > 0)
        {
            var existingSpecial = await context.SpecialDayRates
                .Where(x => x.UnitId != null)
                .Select(x => new { x.UnitId, x.Date })
                .ToListAsync();
            var usedSpecial = new HashSet<string>(existingSpecial.Select(x => $"{x.UnitId}:{x.Date:yyyyMMdd}"));
            var specials = new List<SpecialDayRate>();

            for (var i = 0; i < specialMissing * 3 && specials.Count < specialMissing; i++)
            {
                var unit = faker.PickRandom(unitsList);
                var date = now.Date.AddDays(i);
                var key = $"{unit.Id}:{date:yyyyMMdd}";
                if (!usedSpecial.Add(key))
                {
                    continue;
                }

                specials.Add(new SpecialDayRate
                {
                    Id = Guid.NewGuid(),
                    PropertyId = unit.PropertyId,
                    UnitId = unit.Id,
                    Date = date,
                    Price = faker.Random.Decimal(1200, 10000),
                    CurrencyCode = "TRY",
                    Description = faker.PickRandom("Hafta Sonu", "Resmi Tatil", "Etkinlik Dönemi"),
                    IsActive = true
                });
            }

            context.SpecialDayRates.AddRange(specials);
            await context.SaveChangesAsync();
        }

        var promotionMissing = MinRecordCount - await context.Promotions.CountAsync();
        if (promotionMissing > 0)
        {
            var promotions = new Faker<Promotion>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.PropertyId, f => f.PickRandom(propertyIds))
                .RuleFor(x => x.UnitId, f => f.PickRandom(unitIds))
                .RuleFor(x => x.Code, f => $"PROMO-{f.Random.AlphaNumeric(10).ToUpperInvariant()}")
                .RuleFor(x => x.Name, f => $"{f.Commerce.ProductAdjective()} Kampanya")
                .RuleFor(x => x.Description, f => f.Lorem.Sentence())
                .RuleFor(x => x.Type, f => f.PickRandom<PromotionType>())
                .RuleFor(x => x.DiscountValue, f => f.Random.Decimal(5, 50))
                .RuleFor(x => x.CurrencyCode, _ => "TRY")
                .RuleFor(x => x.MinStayDays, f => f.Random.Int(1, 7))
                .RuleFor(x => x.MaxUsageCount, f => f.Random.Int(10, 1000))
                .RuleFor(x => x.UsedCount, f => f.Random.Int(0, 100))
                .RuleFor(x => x.StartDate, f => now.Date.AddDays(f.Random.Int(-30, 30)))
                .RuleFor(x => x.EndDate, (f, x) => x.StartDate.AddDays(f.Random.Int(30, 365)))
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(promotionMissing);

            context.Promotions.AddRange(promotions);
            await context.SaveChangesAsync();
        }

        var promotionsList = await context.Promotions.ToListAsync();
        var promotionUsageMissing = MinRecordCount - await context.PromotionUsages.CountAsync();
        if (promotionUsageMissing > 0)
        {
            var existingPromotionUsage = await context.PromotionUsages
                .Select(x => new { x.PromotionId, x.ReservationId })
                .ToListAsync();
            var usedPromotionUsage = new HashSet<string>(existingPromotionUsage.Select(x => $"{x.PromotionId}:{x.ReservationId}"));
            var promotionUsages = new List<PromotionUsage>();

            foreach (var promotion in promotionsList.OrderBy(_ => Guid.NewGuid()))
            {
                if (promotionUsages.Count >= promotionUsageMissing)
                {
                    break;
                }

                var reservation = faker.PickRandom(reservationsList);
                var key = $"{promotion.Id}:{reservation.Id}";
                if (!usedPromotionUsage.Add(key))
                {
                    continue;
                }

                promotionUsages.Add(new PromotionUsage
                {
                    Id = Guid.NewGuid(),
                    PromotionId = promotion.Id,
                    ReservationId = reservation.Id,
                    DiscountAmount = faker.Random.Decimal(100, 3000),
                    UsedAt = now
                });
            }

            context.PromotionUsages.AddRange(promotionUsages);
            await context.SaveChangesAsync();
        }

        await EnsureSimpleMinCount(context.CurrencyFormats, MinRecordCount, () =>
        {
            var currencyCode = faker.PickRandom(context.Currencies.Select(x => x.Code).ToList());
            return new CurrencyFormat
            {
                Id = Guid.NewGuid(),
                CurrencyCode = currencyCode,
                Symbol = currencyCode == "TRY" ? "₺" : "$",
                Pattern = "{0} {1}"
            };
        });

        await EnsureSimpleMinCount(context.CalendarBlocks, MinRecordCount, () =>
        {
            var unit = faker.PickRandom(unitsList);
            var start = now.Date.AddDays(faker.Random.Int(-30, 120));
            return new CalendarBlock
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                PropertyId = unit.PropertyId,
                Type = faker.PickRandom<BlockType>(),
                StartDate = start,
                EndDate = start.AddDays(faker.Random.Int(1, 7)),
                Reason = faker.PickRandom("Bakım", "Özel Kullanım", "Teknik Arıza"),
                Notes = faker.Lorem.Sentence(),
                CreatedByTenantId = propertiesList.First(x => x.Id == unit.PropertyId).TenantId,
                CreatedByAgencyId = faker.Random.Bool() ? faker.PickRandom(agencyIds) : null,
                IsActive = true,
                CreatedAt = now
            };
        });

        await EnsureSimpleMinCount(context.CalendarEvents, MinRecordCount, () =>
        {
            var unit = faker.PickRandom(unitsList);
            var start = now.Date.AddDays(faker.Random.Int(-20, 200));
            return new CalendarEvent
            {
                Id = Guid.NewGuid(),
                PropertyId = unit.PropertyId,
                UnitId = unit.Id,
                Title = faker.PickRandom("Temizlik", "Kontrol", "Toplantı", "Transfer"),
                StartDate = start,
                EndDate = start.AddDays(faker.Random.Int(0, 2))
            };
        });

        await EnsureSimpleMinCount(context.CalendarNotes, MinRecordCount, () =>
        {
            var propertyId = faker.PickRandom(propertyIds);
            return new CalendarNote
            {
                Id = Guid.NewGuid(),
                PropertyId = propertyId,
                Date = now.Date.AddDays(faker.Random.Int(-40, 240)),
                Note = faker.Lorem.Sentence()
            };
        });

        await EnsureSimpleMinCount(context.Notifications, MinRecordCount, () =>
        {
            var tenantId = faker.PickRandom(tenantIds);
            return new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AgencyId = faker.Random.Bool() ? faker.PickRandom(agencyIds) : null,
                Type = faker.PickRandom<NotificationType>(),
                Channel = faker.PickRandom<NotificationChannel>(),
                Title = faker.Lorem.Sentence(4),
                Message = faker.Lorem.Paragraph(),
                TemplateCode = "GENERIC",
                RecipientEmail = faker.Internet.Email(),
                RecipientPhone = $"+90{faker.Random.ReplaceNumbers("5#########")}",
                RecipientUserId = null,
                ReferenceId = faker.PickRandom(reservationIds),
                ReferenceType = "Reservation",
                Status = faker.PickRandom<NotificationStatus>(),
                RetryCount = faker.Random.Int(0, 2),
                ErrorMessage = "",
                CreatedAt = now,
                SentAt = now,
                ReadAt = faker.Random.Bool() ? now : null
            };
        });

        var notificationTemplateMissing = MinRecordCount - await context.NotificationTemplates.CountAsync();
        if (notificationTemplateMissing > 0)
        {
            var existingTemplatePairs = await context.NotificationTemplates
                .Select(x => new { x.TenantId, x.Code })
                .ToListAsync();
            var usedTemplates = new HashSet<string>(existingTemplatePairs.Select(x => $"{x.TenantId}:{x.Code}"));
            var templates = new List<NotificationTemplate>();

            for (var i = 0; i < notificationTemplateMissing * 2 && templates.Count < notificationTemplateMissing; i++)
            {
                var tenantId = faker.PickRandom(tenantIds);
                var code = $"TPL-{i:0000}";
                var key = $"{tenantId}:{code}";
                if (!usedTemplates.Add(key))
                {
                    continue;
                }

                templates.Add(new NotificationTemplate
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Code = code,
                    Name = $"Bildirim Şablonu {i + 1}",
                    Type = faker.PickRandom<NotificationType>(),
                    Subject = "Bilgilendirme",
                    BodyTemplate = "{{message}}",
                    SMSTemplate = "{{message}}",
                    AvailableVariables = "[\"message\"]",
                    IsActive = true,
                    CreatedAt = now
                });
            }

            context.NotificationTemplates.AddRange(templates);
            await context.SaveChangesAsync();
        }

        var notificationPrefMissing = MinRecordCount - await context.NotificationPreferences.CountAsync();
        if (notificationPrefMissing > 0)
        {
            var usedPrefTenants = await context.NotificationPreferences.Select(x => x.TenantId).ToListAsync();
            var prefTenantIds = tenantIds.Except(usedPrefTenants).Take(notificationPrefMissing).ToList();
            context.NotificationPreferences.AddRange(prefTenantIds.Select(tid => new NotificationPreference
            {
                Id = Guid.NewGuid(),
                TenantId = tid,
                Type = NotificationType.NewReservation,
                EmailAddresses = "[\"ops@trimango.local\"]",
                PhoneNumbers = "[\"+905551112233\"]",
                EmailEnabled = true,
                SMSEnabled = true,
                InAppEnabled = true,
                PushEnabled = false
            }));
            await context.SaveChangesAsync();
        }

        await EnsureSimpleMinCount(context.NotificationLogs, MinRecordCount, () =>
        {
            var notificationId = faker.PickRandom(context.Notifications.Select(x => x.Id).ToList());
            return new NotificationLog
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                Channel = faker.PickRandom("Email", "SMS", "InApp", "Push"),
                Status = faker.PickRandom("Sent", "Failed", "Pending"),
                CreatedAt = now
            };
        });

        var bookingWidgetMissing = MinRecordCount - await context.BookingWidgets.CountAsync();
        if (bookingWidgetMissing > 0)
        {
            var widgets = new Faker<BookingWidget>("tr")
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.PropertyId, f => f.PickRandom(propertyIds))
                .RuleFor(x => x.WidgetKey, f => $"WIDGET-{f.Random.AlphaNumeric(12).ToUpperInvariant()}")
                .RuleFor(x => x.Theme, f => f.PickRandom("default", "modern", "minimal"))
                .RuleFor(x => x.PrimaryColor, _ => "#2563EB")
                .RuleFor(x => x.SecondaryColor, _ => "#1E40AF")
                .RuleFor(x => x.FontFamily, _ => "Inter, sans-serif")
                .RuleFor(x => x.ShowPropertyImages, _ => true)
                .RuleFor(x => x.ShowAmenities, _ => true)
                .RuleFor(x => x.ShowReviews, _ => true)
                .RuleFor(x => x.ShowPriceBreakdown, _ => true)
                .RuleFor(x => x.Position, f => f.PickRandom<WidgetPosition>())
                .RuleFor(x => x.CustomCSS, _ => "")
                .RuleFor(x => x.MetaTitle, _ => "Trimango Rezervasyon")
                .RuleFor(x => x.MetaDescription, _ => "Online rezervasyon")
                .RuleFor(x => x.SharingImage, _ => "/images/share.jpg")
                .RuleFor(x => x.RequirePayment, f => f.Random.Bool())
                .RuleFor(x => x.MinAdvanceDays, f => f.Random.Int(0, 7))
                .RuleFor(x => x.MaxAdvanceDays, f => f.Random.Int(60, 365))
                .RuleFor(x => x.DefaultLanguage, _ => "tr")
                .RuleFor(x => x.AvailableLanguages, _ => "[\"tr\",\"en\"]")
                .RuleFor(x => x.IsActive, _ => true)
                .RuleFor(x => x.CreatedAt, _ => now)
                .Generate(bookingWidgetMissing);

            context.BookingWidgets.AddRange(widgets);
            await context.SaveChangesAsync();
        }

        var widgetsList = await context.BookingWidgets.ToListAsync();
        await EnsureSimpleMinCount(context.WidgetIntegrations, MinRecordCount, () =>
        {
            var widget = faker.PickRandom(widgetsList);
            return new WidgetIntegration
            {
                Id = Guid.NewGuid(),
                WidgetId = widget.Id,
                Domain = $"seed-{Guid.NewGuid():N}.example.com",
                IsActive = true,
                CreatedAt = now
            };
        });

        await EnsureSimpleMinCount(context.WidgetThemes, MinRecordCount, () =>
        {
            var widget = faker.PickRandom(widgetsList);
            return new WidgetTheme
            {
                Id = Guid.NewGuid(),
                WidgetId = widget.Id,
                Name = faker.PickRandom("default", "modern", "dark", "minimal"),
                ConfigJson = "{\"radius\":8}"
            };
        });

        await EnsureSimpleMinCount(context.Reports, MinRecordCount, () =>
        {
            var tenantId = faker.PickRandom(tenantIds);
            var start = now.Date.AddDays(-faker.Random.Int(30, 180));
            return new Report
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AgencyId = faker.Random.Bool() ? faker.PickRandom(agencyIds) : null,
                Name = $"Rapor {faker.Random.Int(1, 9999)}",
                Type = faker.PickRandom<ReportType>(),
                Period = faker.PickRandom<ReportPeriod>(),
                StartDate = start,
                EndDate = start.AddDays(faker.Random.Int(1, 30)),
                Filters = "{}",
                FilePath = $"/reports/{Guid.NewGuid():N}.xlsx",
                FileFormat = "Excel",
                FileSize = faker.Random.Long(10_000, 1_000_000),
                Status = faker.PickRandom<ReportStatus>(),
                ErrorMessage = "",
                CreatedAt = now,
                CompletedAt = now,
                CreatedBy = "seed"
            };
        });

        await EnsureSimpleMinCount(context.ReportSchedules, MinRecordCount, () => new ReportSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = faker.PickRandom(tenantIds),
            ReportType = faker.PickRandom("Revenue", "Occupancy", "Reservation"),
            CronExpression = "0 0 * * *",
            IsActive = true
        });

        await EnsureSimpleMinCount(context.DashboardWidgets, MinRecordCount, () => new DashboardWidget
        {
            Id = Guid.NewGuid(),
            TenantId = faker.PickRandom(tenantIds),
            WidgetType = faker.PickRandom("Revenue", "Occupancy", "Reservations"),
            ConfigJson = "{\"period\":\"monthly\"}",
            SortOrder = faker.Random.Int(1, 20)
        });

        await EnsureSimpleMinCount(context.AuditLogs, MinRecordCount, () => new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = faker.PickRandom(tenantIds),
            EntityName = faker.PickRandom("Reservation", "Property", "Agency", "Guest"),
            EntityId = Guid.NewGuid().ToString("N"),
            Action = faker.PickRandom("Create", "Update", "Delete"),
            OldValues = "{}",
            NewValues = "{}",
            UserId = "seed",
            IpAddress = "127.0.0.1",
            Data = "{}",
            CreatedAt = now,
            CreatedBy = "seed"
        });

        await EnsureSimpleMinCount(context.SystemSettings, MinRecordCount, () => new SystemSetting
        {
            Id = Guid.NewGuid(),
            Key = $"setting:{Guid.NewGuid():N}",
            Value = faker.Lorem.Word()
        });

        await EnsureSimpleMinCount(context.EmailTemplates, MinRecordCount, () => new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Code = $"EMAIL-{Guid.NewGuid():N}",
            Subject = faker.Lorem.Sentence(4),
            Body = faker.Lorem.Paragraph(),
            IsActive = true
        });

        await EnsureSimpleMinCount(context.SmsTemplates, MinRecordCount, () => new SmsTemplate
        {
            Id = Guid.NewGuid(),
            Code = $"SMS-{Guid.NewGuid():N}",
            Body = faker.Lorem.Sentence(),
            IsActive = true
        });
    }

    private static async Task SeedExchangeRates(AppDbContext context)
    {
        var currentCount = await context.ExchangeRates.CountAsync();
        var missing = MinRecordCount - currentCount;
        if (missing <= 0)
        {
            return;
        }

        var currencyCodes = await context.Currencies.Select(x => x.Code).ToListAsync();
        var baseCodes = currencyCodes.Where(c => c != "TRY").Take(20).ToList();
        if (baseCodes.Count == 0)
        {
            baseCodes.Add("USD");
        }

        var existingKeys = await context.ExchangeRates
            .Select(x => new { x.BaseCurrencyCode, x.TargetCurrencyCode, x.Date })
            .ToListAsync();
        var used = new HashSet<string>(existingKeys.Select(x => $"{x.BaseCurrencyCode}:{x.TargetCurrencyCode}:{x.Date:yyyyMMdd}"));
        var rows = new List<ExchangeRate>();
        var day = DateTime.UtcNow.Date.AddDays(-365);

        while (rows.Count < missing)
        {
            foreach (var baseCode in baseCodes)
            {
                if (rows.Count >= missing)
                {
                    break;
                }

                var key = $"{baseCode}:TRY:{day:yyyyMMdd}";
                if (!used.Add(key))
                {
                    continue;
                }

                var rate = 20m + (decimal)(day.DayOfYear % 180) / 10m;
                rows.Add(new ExchangeRate
                {
                    BaseCurrencyCode = baseCode,
                    TargetCurrencyCode = "TRY",
                    Rate = rate,
                    BuyRate = rate - 0.1m,
                    SellRate = rate + 0.1m,
                    Date = day,
                    Source = "Seed",
                    UpdatedAt = DateTime.UtcNow
                });
            }

            day = day.AddDays(1);
        }

        context.ExchangeRates.AddRange(rows);
    }

    private static async Task EnsureSimpleMinCount<T>(
        DbSet<T> dbSet,
        int minCount,
        Func<T> factory) where T : class
    {
        var count = await dbSet.CountAsync();
        var missing = minCount - count;
        if (missing <= 0)
        {
            return;
        }

        var rows = new List<T>();
        for (var i = 0; i < missing; i++)
        {
            rows.Add(factory());
        }

        await dbSet.AddRangeAsync(rows);
    }
}
