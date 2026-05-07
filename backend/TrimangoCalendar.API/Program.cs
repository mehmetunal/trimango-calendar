// backend/TrimangoCalendar.API/Program.cs
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using Hangfire;
using Hangfire.SqlServer;
using FluentValidation;
using FluentValidation.AspNetCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Core.Services;
using TrimangoCalendar.Core.Validators;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;
using TrimangoCalendar.Data.Repositories.Tenant;
using TrimangoCalendar.Data.Repositories.Property;
using TrimangoCalendar.Data.Repositories.Unit;
using TrimangoCalendar.Data.Repositories.Reservation;
using TrimangoCalendar.Data.Repositories.Guest;
using TrimangoCalendar.Data.Repositories.Agency;
using TrimangoCalendar.Data.Repositories.Pricing;
using TrimangoCalendar.Data.Repositories.Calendar;
using TrimangoCalendar.Data.Repositories.Notification;
using TrimangoCalendar.Data.UnitOfWork;
using TrimangoCalendar.API.Middleware;
using TrimangoCalendar.API.Filters;
using TrimangoCalendar.API.BackgroundJobs;
using TrimangoCalendar.Shared.Helpers;
using TrimangoCalendar.Shared.Extensions;

// ==========================================
// BUILDER - SERVİS YAPILANDIRMASI
// ==========================================
var builder = WebApplication.CreateBuilder(args);

// --- SERILOG ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// --- VERİTABANI (Entity Framework Core) ---
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("TrimangoCalendar.Data");
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    });
    options.UseLazyLoadingProxies();
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// --- IDENTITY (ASP.NET Core Identity) ---
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 3;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>();

// Identity cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "TrimangoCalendar.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
});

// --- JWT AUTHENTICATION ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero, // Token süresi tam zamanında dolsun
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Token doğrulandıktan sonra ek kontroller yapılabilir
            return Task.CompletedTask;
        }
    };
});

// --- AUTHORIZATION POLICIES ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TenantOwner", policy => policy.RequireRole("Admin", "TenantOwner"));
    options.AddPolicy("AgencyUser", policy => policy.RequireRole("Admin", "AgencyUser"));
    options.AddPolicy("TenantOrAgency", policy => policy.RequireRole("Admin", "TenantOwner", "AgencyUser"));
    
    options.AddPolicy("CanManageProperties", policy =>
        policy.RequireRole("Admin", "TenantOwner"));
    
    options.AddPolicy("CanManageReservations", policy =>
        policy.RequireRole("Admin", "TenantOwner", "AgencyUser"));
    
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("Admin", "TenantOwner", "AgencyUser"));
});

// --- REDIS CACHE ---
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "TrimangoCalendar_";
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse(redisConnectionString);
        configuration.AbortOnConnectFail = false;
        configuration.ConnectTimeout = 5000;
        configuration.SyncTimeout = 5000;
        return ConnectionMultiplexer.Connect(configuration);
    });
}

// --- MEMORY CACHE (Fallback) ---
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// --- SESSION ---
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// --- HANGFIRE (Background Jobs) ---
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(
              builder.Configuration.GetConnectionString("DefaultConnection"),
              new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true,
                  PrepareSchemaIfNecessary = true,
              });
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"TrimangoCalendar-{Guid.NewGuid():N}";
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.Queues = new[] { "default", "critical", "emails", "reports" };
});

// --- REPOSITORIES (DI) ---
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IAgencyRepository, AgencyRepository>();
builder.Services.AddScoped<IPricingRepository, PricingRepository>();
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// --- UNIT OF WORK ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- SERVICES (DI) ---
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBookingEngineService, BookingEngineService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// --- BACKGROUND JOBS ---
builder.Services.AddScoped<ExchangeRateUpdateJob>();
builder.Services.AddScoped<ReservationReminderJob>();
builder.Services.AddScoped<ReportGenerationJob>();
builder.Services.AddScoped<DataCleanupJob>();

// --- AUTO MAPPER ---
builder.Services.AddAutoMapper(typeof(Program));

// --- FLUENT VALIDATION ---
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();

// --- MEDIATR (CQRS) ---
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ITenantService).Assembly);
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://localhost:5173",
                "https://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Token-Expired", "X-Pagination", "X-Total-Count")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    options.AddPolicy("AllowWidget", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- RESPONSE COMPRESSION ---
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/plain", "text/html" }
    );
});

// --- RATE LIMITING ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 5;
    });
});

// --- HEALTH CHECKS ---
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database")
    .AddRedis(redisConnectionString ?? "localhost:6379", "Redis", tags: new[] { "cache" });

// --- CONTROLLERS ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidateModelFilter>();
    options.Filters.Add<AddTenantIdFilter>();
    options.Filters.Add<LogRequestFilter>();
    options.CacheProfiles.Add("Default", new CacheProfile
    {
        Duration = 60,
        Location = ResponseCacheLocation.Any
    });
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
})
.AddXmlSerializerFormatters()
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// --- API VERSIONING ---
builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
});

// --- SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrimangoCalendar API",
        Version = "v1",
        Description = "HotelRunner benzeri SaaS kiralama yönetim platformu API'si",
        Contact = new OpenApiContact
        {
            Name = "TrimangoCalendar Team",
            Email = "info@trimangocalendar.com",
            Url = new Uri("https://trimangocalendar.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\n\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\n\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML Comments
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.EnableAnnotations();
});

// --- HTTP CLIENT ---
builder.Services.AddHttpClient("TCMB", client =>
{
    client.BaseAddress = new Uri("https://www.tcmb.gov.tr/kurlar/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/xml");
    client.DefaultRequestHeaders.Add("User-Agent", "TrimangoCalendar/1.0");
});

builder.Services.AddHttpClient("ExchangeRateAPI", client =>
{
    client.BaseAddress = new Uri("https://api.exchangerate-api.com/v4/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// --- OPTIONS PATTERN ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

// ==========================================
// APP - MIDDLEWARE PIPELINE
// ==========================================
var app = builder.Build();

// --- GELİŞTİRME / PRODÜKSİYON AYARLARI ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrimangoCalendar API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "TrimangoCalendar API Documentation";
        options.DefaultModelsExpandDepth(-1); // Modelleri gizle
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// --- SERILOG REQUEST LOGGING ---
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

// --- HTTPS REDIRECTION ---
app.UseHttpsRedirection();

// --- STATIC FILES ---
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

// --- RESPONSE COMPRESSION ---
app.UseResponseCompression();

// --- CORS ---
app.UseCors("AllowFrontend");

// --- RATE LIMITING ---
app.UseRateLimiter();

// --- SESSION ---
app.UseSession();

// --- AUTHENTICATION & AUTHORIZATION ---
app.UseAuthentication();
app.UseAuthorization();

// --- TENANT MIDDLEWARE ---
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// --- HANGFIRE DASHBOARD ---
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "TrimangoCalendar Jobs",
    Authorization = new[] { new HangfireAuthorizationFilter() },
    StatsPollingInterval = 5000,
    DisplayStorageConnectionString = false,
    DarkModeEnabled = true,
});

// --- MAP CONTROLLERS ---
app.MapControllers();

// --- HEALTH CHECKS ---
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        });
        await context.Response.WriteAsync(result);
    }
});

// --- ERROR HANDLING ---
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception occurred");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var errorResponse = new
        {
            success = false,
            message = "Internal server error",
            errorId = Guid.NewGuid().ToString()
        };
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// --- 404 HANDLER ---
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        var notFoundResponse = new
        {
            success = false,
            message = "Endpoint not found",
            path = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(notFoundResponse);
    }
});

// ==========================================
// SEED DATA & BACKGROUND JOBS
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // Veritabanını migrate et
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        
        // Seed data
        await Data.SeedData.InitializeAsync(services);
        
        Log.Information("Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database");
    }
    
    try
    {
        // Hangfire recurring jobs
        var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();
        
        // Döviz kurlarını her gün saat 10:00'da güncelle
        recurringJobManager.AddOrUpdate<ExchangeRateUpdateJob>(
            "update-exchange-rates",
            job => job.ExecuteAsync(),
            "0 10 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        
        // Rezervasyon hatırlatmaları her saat
        recurringJobManager.AddOrUpdate<ReservationReminderJob>(
            "reservation-reminders",
            job => job.ExecuteAsync(),
            "0 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        
        // Günlük rapor oluşturma (sabah 6:00)
        recurringJobManager.AddOrUpdate<ReportGenerationJob>(
            "generate-daily-reports",
            job => job.ExecuteAsync(),
            "0 6 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        
        // Haftalık veri temizliği (Pazar 3:00)
        recurringJobManager.AddOrUpdate<DataCleanupJob>(
            "data-cleanup",
            job => job.ExecuteAsync(),
            "0 3 * * 0",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        
        Log.Information("Hangfire recurring jobs scheduled successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while scheduling Hangfire jobs");
    }
}

// ==========================================
// APPLICATION START
// ==========================================
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