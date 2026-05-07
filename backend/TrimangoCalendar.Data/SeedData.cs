using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;

namespace TrimangoCalendar.Data;

public static class SeedData
{
    public static Task InitializeAsync(IServiceProvider serviceProvider) => Initialize(serviceProvider);

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedCurrencies(context);
        await SeedExchangeRates(context);
        await SeedIdentity(roleManager, userManager, context);
        await SeedDomainData(context);
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
                await roleManager.CreateAsync(new ApplicationRole { Id = Guid.NewGuid(), Name = roleName, NormalizedName = roleName.ToUpperInvariant() });
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
                IsActive = true,
                Plan = "Pro",
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

    private static async Task SeedDomainData(AppDbContext context)
    {
        if (await context.Reservations.AnyAsync())
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(t => t.Subdomain == "demo");

        var propertyFaker = new Faker<Property>("tr")
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Type, f => f.PickRandom<PropertyType>())
            .RuleFor(x => x.Name, f => $"{f.Address.City()} Residence")
            .RuleFor(x => x.Slug, (f, x) => x.Name.ToLowerInvariant().Replace(" ", "-"))
            .RuleFor(x => x.City, f => f.Address.City())
            .RuleFor(x => x.Country, _ => "Türkiye")
            .RuleFor(x => x.IsActive, _ => true)
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow);

        var properties = propertyFaker.Generate(6);
        context.Properties.AddRange(properties);
        await context.SaveChangesAsync();

        var unitFaker = new Faker<Unit>("tr")
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.PropertyId, f => f.PickRandom(properties).Id)
            .RuleFor(x => x.Name, f => $"{f.Commerce.ProductAdjective()} Oda")
            .RuleFor(x => x.UnitNumber, f => f.Random.Int(100, 999).ToString())
            .RuleFor(x => x.BasePrice, f => f.Random.Decimal(1200, 6500))
            .RuleFor(x => x.CurrencyCode, _ => "TRY")
            .RuleFor(x => x.MaxAdults, f => f.Random.Int(1, 4))
            .RuleFor(x => x.MaxChildren, f => f.Random.Int(0, 2))
            .RuleFor(x => x.IsActive, _ => true)
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow);

        var units = unitFaker.Generate(24);
        context.Units.AddRange(units);
        await context.SaveChangesAsync();

        var guestFaker = new Faker<Guest>("tr")
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Email, (f, x) => f.Internet.Email(x.FirstName, x.LastName))
            .RuleFor(x => x.Phone, f => $"+90{f.Random.ReplaceNumbers("5#########")}")
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow);

        var guests = guestFaker.Generate(80);
        context.Guests.AddRange(guests);
        await context.SaveChangesAsync();

        var reservationFaker = new Faker<Reservation>("tr")
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.UnitId, f => f.PickRandom(units).Id)
            .RuleFor(x => x.GuestId, f => f.PickRandom(guests).Id)
            .RuleFor(x => x.ReservationNumber, f => $"R{DateTime.UtcNow:yyyyMMdd}-{f.Random.Int(1000, 9999)}")
            .RuleFor(x => x.CheckIn, f => DateTime.UtcNow.Date.AddDays(f.Random.Int(-30, 120)))
            .RuleFor(x => x.CheckOut, (f, x) => x.CheckIn.AddDays(f.Random.Int(1, 10)))
            .RuleFor(x => x.TotalNights, (f, x) => (x.CheckOut - x.CheckIn).Days)
            .RuleFor(x => x.Adults, f => f.Random.Int(1, 4))
            .RuleFor(x => x.Children, f => f.Random.Int(0, 2))
            .RuleFor(x => x.Infants, f => f.Random.Int(0, 1))
            .RuleFor(x => x.CurrencyCode, _ => "TRY")
            .RuleFor(x => x.TotalAmount, f => f.Random.Decimal(2500, 25000))
            .RuleFor(x => x.PaidAmount, (f, x) => Math.Round(x.TotalAmount * f.Random.Decimal(0.1m, 1m), 2))
            .RuleFor(x => x.RemainingAmount, (f, x) => x.TotalAmount - x.PaidAmount)
            .RuleFor(x => x.Status, f => f.PickRandom<ReservationStatus>())
            .RuleFor(x => x.Source, f => f.PickRandom<ReservationSource>())
            .RuleFor(x => x.CreatedBy, _ => "seed")
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow);

        var reservations = reservationFaker.Generate(140);
        context.Reservations.AddRange(reservations);
    }

    private static async Task SeedCurrencies(AppDbContext context)
    {
        if (await context.Currencies.AnyAsync())
        {
            return;
        }

        context.Currencies.AddRange(
            new Currency { Code = "TRY", Symbol = "₺", Name = "Türk Lirası", IsBaseCurrency = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Currency { Code = "USD", Symbol = "$", Name = "US Dollar", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Currency { Code = "EUR", Symbol = "€", Name = "Euro", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Currency { Code = "GBP", Symbol = "£", Name = "Pound Sterling", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }

    private static async Task SeedExchangeRates(AppDbContext context)
    {
        if (await context.ExchangeRates.AnyAsync())
        {
            return;
        }

        var start = DateTime.UtcNow.Date.AddDays(-30);
        var rows = new List<ExchangeRate>();
        for (var i = 0; i < 60; i++)
        {
            var day = start.AddDays(i);
            rows.Add(new ExchangeRate { BaseCurrencyCode = "USD", TargetCurrencyCode = "TRY", Rate = 31.50m + i * 0.03m, BuyRate = 31.30m + i * 0.03m, SellRate = 31.70m + i * 0.03m, Date = day, Source = "Seed", UpdatedAt = DateTime.UtcNow });
            rows.Add(new ExchangeRate { BaseCurrencyCode = "EUR", TargetCurrencyCode = "TRY", Rate = 34.20m + i * 0.04m, BuyRate = 34.00m + i * 0.04m, SellRate = 34.40m + i * 0.04m, Date = day, Source = "Seed", UpdatedAt = DateTime.UtcNow });
        }
        context.ExchangeRates.AddRange(rows);
    }
}
