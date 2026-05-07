using FluentMigrator.Runner;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace TrimangoCalendar.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// InitializeDatabaseAsync methodunu çalıştırır.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Development ortamında DB kurulumu/seed işlemi manuel olarak /api/admin/seed endpoint'inden tetiklenir.
            return;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var migrationRunner = services.GetRequiredService<IMigrationRunner>();
            migrationRunner.MigrateUp();
            await TrimangoCalendar.Data.SeedData.InitializeAsync(services);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "FluentMigrator migration or seed failed");
        }
    }

    /// <summary>
    /// ConfigureHttpPipeline methodunu çalıştırır.
    /// </summary>
    public static WebApplication ConfigureHttpPipeline(this WebApplication app)
    {
        var enableSwagger = app.Configuration.GetValue<bool>("EnableSwagger");
        if (app.Environment.IsDevelopment() || enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions());
        var hangfireEnabled = app.Configuration.GetValue<bool>("Hangfire:Enabled");
        if (hangfireEnabled)
        {
            app.UseHangfireDashboard("/hangfire");
        }
        return app;
    }
}
