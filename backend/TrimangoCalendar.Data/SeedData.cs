// Data/SeedData.cs
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Admin tenant'ı oluştur
        if (!await context.Tenants.AnyAsync())
        {
            var adminTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Platform Admin",
                Subdomain = "admin",
                Email = "admin@hotelplatform.com",
                Phone = "05555555555",
                Plan = "Enterprise",
                PlanStartDate = DateTime.UtcNow,
                MaxProperties = int.MaxValue,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Tenants.Add(adminTenant);
            await context.SaveChangesAsync();
            
            // Admin kullanıcı oluştur
            var adminUser = new ApplicationUser
            {
                UserName = "admin@hotelplatform.com",
                Email = "admin@hotelplatform.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                TenantId = adminTenant.Id
            };
            
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        
        // Demo tenant oluştur
        if (!await context.Tenants.AnyAsync(t => t.Subdomain == "demo"))
        {
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
            await context.SaveChangesAsync();
        }
    }
}