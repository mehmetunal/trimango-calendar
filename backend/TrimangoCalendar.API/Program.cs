using Serilog;
using TrimangoCalendar.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.InitializeDatabaseAsync();
app.ConfigureHttpPipeline();

try
{
    Log.Information("TrimangoCalendar API starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
