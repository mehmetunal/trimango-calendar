builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHangfireServer();

// Job scheduling
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Her gün saat 10:00'da TCMB kurlarını güncelle
RecurringJob.AddOrUpdate<ExchangeRateUpdateJob>(
    "update-exchange-rates",
    job => job.Execute(),
    "0 10 * * *"); // Cron expression: Her gün 10:00
