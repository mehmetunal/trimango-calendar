// Web/Middleware/TenantMiddleware.cs
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Admin panelinde tenant'a gerek yok
        if (context.Request.Path.StartsWithSegments("/admin") || 
            context.Request.Path.StartsWithSegments("/api/admin"))
        {
            await _next(context);
            return;
        }
        
        // Subdomain'den tenant'ı bul
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        
        if (parts.Length > 2 && parts[0] != "www")
        {
            var subdomain = parts[0];
            var tenant = await tenantService.GetBySubdomainAsync(subdomain);
            
            if (tenant != null)
            {
                context.Items["TenantId"] = tenant.Id;
                context.Items["Tenant"] = tenant;
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant bulunamadı");
                return;
            }
        }
        
        await _next(context);
    }
}