using System.Text;
using System.Text.Json.Serialization;
using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Interfaces;
using TrimangoCalendar.Data.Repositories.Agency;
using TrimangoCalendar.Data.Repositories.Base;
using TrimangoCalendar.Data.Repositories.Calendar;
using TrimangoCalendar.Data.Repositories.Guest;
using TrimangoCalendar.Data.Repositories.Notification;
using TrimangoCalendar.Data.Repositories.Pricing;
using TrimangoCalendar.Data.Repositories.Property;
using TrimangoCalendar.Data.Repositories.Reservation;
using TrimangoCalendar.Data.Repositories.Tenant;
using TrimangoCalendar.Data.Repositories.Unit;
using TrimangoCalendar.Infrastructure.Services;

namespace TrimangoCalendar.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// AddSerilogLogging methodunu çalıştırır.
    /// </summary>
    public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .CreateLogger();

        hostBuilder.UseSerilog();
        return hostBuilder;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDatabase(configuration, environment);
        services.AddIdentityAndAuth(configuration);
        services.AddCaching(configuration);
        services.AddHangfire(configuration);
        services.AddRepositoriesAndDomainServices();
        services.AddApiInfrastructure(configuration, environment);
        return services;
    }

    /// <summary>
    /// AddDatabase methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("TrimangoCalendar.Data");
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                sqlOptions.CommandTimeout(60);
            });
            options.UseLazyLoadingProxies();
            options.EnableSensitiveDataLogging(environment.IsDevelopment());
            options.EnableDetailedErrors(environment.IsDevelopment());
        });

        return services;
    }

    /// <summary>
    /// AddIdentityAndAuth methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddIdentityAndAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// AddCaching methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is required.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TrimangoCalendar_";
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var cfg = ConfigurationOptions.Parse(redisConnectionString);
            cfg.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(cfg);
        });

        return services;
    }

    /// <summary>
    /// AddHangfire methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireEnabled = configuration.GetValue<bool>("Hangfire:Enabled");
        if (!hangfireEnabled)
        {
            return services;
        }

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions());
        });
        services.AddHangfireServer();
        return services;
    }

    /// <summary>
    /// AddRepositoriesAndDomainServices methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddRepositoriesAndDomainServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IPricingRepository, PricingRepository>();
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpClient<ISmsService, SmsService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }

    /// <summary>
    /// AddApiInfrastructure methodunu çalıştırır.
    /// </summary>
    private static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddAutoMapper(typeof(Program));
        services.AddMediatR(typeof(Program).Assembly);

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddSqlServer()
                .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(Program).Assembly).For.Migrations());

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("Database");
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.WriteIndented = environment.IsDevelopment();
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TrimangoCalendar API", Version = "v1" });
        });

        return services;
    }
}
