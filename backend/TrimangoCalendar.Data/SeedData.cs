using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrimangoCalendar.Core.Entities;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedTenants(context);
        await SeedDemoProperties(context);
        await SeedCurrencies(context);
        await SeedExchangeRates(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedTenants(AppDbContext context)
    {
        if (await context.Tenants.AnyAsync(t => t.Subdomain == "demo"))
        {
            return;
        }

        var demo = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Demo Otel",
            Subdomain = "demo",
            Email = "info@demootel.com",
            Phone = "02125555555",
            Plan = "Free",
            PlanStartDate = DateTime.UtcNow,
            MaxProperties = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Tenants.Add(demo);
    }

    private static async Task SeedDemoProperties(AppDbContext context)
    {
        var demoTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "demo");
        if (demoTenant == null || await context.Properties.AnyAsync(p => p.TenantId == demoTenant.Id))
        {
            return;
        }

        var property = new Property
        {
            Id = Guid.NewGuid(),
            TenantId = demoTenant.Id,
            Name = "Demo Property",
            Slug = "demo-property",
            City = "Antalya",
            Country = "Türkiye",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Properties.Add(property);
    }

    private static async Task SeedCurrencies(AppDbContext context)
    {
        if (await context.Currencies.AnyAsync())
        {
            return;
        }

        context.Currencies.AddRange(
            new Currency { Code = "TRY", Symbol = "₺", Name = "Türk Lirası", IsBaseCurrency = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Currency { Code = "USD", Symbol = "$", Name = "Amerikan Doları", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Currency { Code = "EUR", Symbol = "€", Name = "Euro", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }

    private static async Task SeedExchangeRates(AppDbContext context)
    {
        if (await context.ExchangeRates.AnyAsync())
        {
            return;
        }

        var today = DateTime.UtcNow.Date;
        context.ExchangeRates.AddRange(
            new ExchangeRate { BaseCurrencyCode = "USD", TargetCurrencyCode = "TRY", Rate = 32.50m, Date = today, UpdatedAt = DateTime.UtcNow },
            new ExchangeRate { BaseCurrencyCode = "EUR", TargetCurrencyCode = "TRY", Rate = 35.20m, Date = today, UpdatedAt = DateTime.UtcNow },
            new ExchangeRate { BaseCurrencyCode = "TRY", TargetCurrencyCode = "USD", Rate = 0.0308m, Date = today, UpdatedAt = DateTime.UtcNow },
            new ExchangeRate { BaseCurrencyCode = "TRY", TargetCurrencyCode = "EUR", Rate = 0.0284m, Date = today, UpdatedAt = DateTime.UtcNow }
        );
    }
}
