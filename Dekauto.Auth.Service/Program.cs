using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Dekauto.Auth.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using System.Text;
using System.Text.Json.Serialization;

var tempOutputTemplate = "[AUTH STARTUP LOGGER] {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
// Временные логгер Serilog для этапа до создания билдера
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal) // Только критические ошибки из Microsoft-сервисов
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: tempOutputTemplate,
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .WriteTo.File(
        "logs/Auth-startup-log.txt",
        outputTemplate: tempOutputTemplate,
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning
    )
    .CreateBootstrapLogger(); // временный логгер

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Применение конфигов.
    builder.Configuration
        .AddEnvironmentVariables()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
        .AddCommandLine(args);

    // Полноценная настройка Serilog логгера (из конфига)
    builder.Host.UseSerilog((builderContext, serilogConfig) =>
        {
            serilogConfig
                .ReadFrom.Configuration(builderContext.Configuration)
                // Ручная настройка Loki
                .WriteTo.GrafanaLoki(
                    uri: "http://loki:3100",
                    labels: new List<LokiLabel>
                    {
                        new LokiLabel { Key = "app", Value = "dekauto_auth" },
                        new LokiLabel { Key = "app_full", Value = "dekauto_full" }
                    },
                    textFormatter: new LokiJsonTextFormatter()
                );
        });

    builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("Jwt__Key");
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        var mes = "Invalid secret key for JWT tokens - needs to be at least 32 characters long.";
        Log.Fatal(mes);
        throw new InvalidOperationException(mes);
    }

    var connectionString = builder.Configuration.GetConnectionString("Main");

    // Получаем список origins из конфигурации
    var allowedOrigins = builder.Configuration
        .GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

    if (allowedOrigins == null || !allowedOrigins.Any())
    {
        var mes =
            "CORS AllowedOrigins are not specified in serilogConfig (appsettings.json or environment). Can't configure CORS";
        Log.Error(mes);
        throw new InvalidOperationException(mes);
    }

    if (Boolean.Parse(builder.Configuration["UseEndpointAuth"] ?? "true"))
    {

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Политики доступа к эндпоинтам
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("OnlyAdmin", policy => policy.RequireRole("Admin"));

        builder.Services.AddAuthorization();
    }
    else
    {
        // Заглушка политик доступа, если авторизация выключена
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("OnlyAdmin", policy => policy.RequireAssertion(_ => true));
    }

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    // Добавление swagger с авторизацией
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Input JWT token (without 'Bearer')",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
        });
    });
    // Добавляем JWT сервис
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
    builder.Services.AddTransient<IUserAuthService, UserAuthService>();
    builder.Services.AddTransient<IUsersRepository, UsersRepository>();
    builder.Services.AddTransient<IRolesRepository, RolesRepository>();
    builder.Services.AddTransient<IRolesService, RolesService>();
    builder.Services.AddSingleton<IRequestMetricsService, RequestMetricsService>();
    builder.Services.AddDbContext<DekautoContext>(options =>
        options.UseNpgsql(connectionString)
        .UseLazyLoadingProxies());
    builder.Services.AddCors(options => options.AddPolicy("AllowMainHosts", policy =>
    {
        policy.WithOrigins(allowedOrigins)
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .WithExposedHeaders("Content-Disposition")
                 .AllowCredentials();
    }));


    Log.Information("Building the application...");
    var app = builder.Build();

    // Configure the HTTP request pipeline.

    // Явно указываем порты (для Docker)
    app.Urls.Add("http://*:5507");

    app.UseCors("AllowMainHosts");

    if (app.Environment.IsDevelopment())
    {
        Log.Warning("Development version of the application is started. Swagger activation...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Включаем https, если указано в конфиге
    if (Boolean.Parse(app.Configuration["UseHttps"] ?? "false"))
    {
        app.Urls.Add("https://*:5508");
        app.UseHttpsRedirection();
        Log.Information("Enabled HTTPS.");
    }
    else
    {
        Log.Warning("Disabled HTTPS.");
    }

    if (Boolean.Parse(app.Configuration["UseEndpointAuth"] ?? "true"))
    {
        // Аутентификация (JWT, куки)
        app.UseAuthentication();

        // Авторизация (проверка атрибутов [Authorize])
        app.UseAuthorization();
    }
    else
    {
        Log.Warning("Disabled all endpoint authorization.");
    }
    app.MapControllers();

    app.MapMetrics();
    app.UseMetricsMiddleware(); // Метрики

    Log.Information("Application startup...");
    app.Run();
}
catch (Exception ex)
{
    // В случае краха приложения при запуске пытаемся отправить логи:
    // 1. Запись в файл и консоль контейнера
    Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
    try
    {
        // 2. Попытка отправить критическую ошибку в Loki
        using var tempLogger = new LoggerConfiguration()
            .WriteTo.GrafanaLoki(
                "http://loki:3100",
                labels: new List<LokiLabel>
                {
                    new LokiLabel { Key = "app_startup", Value = "dekauto_auth_startup" },
                    new LokiLabel { Key = "app_full", Value = "dekauto_full" }
                },
                textFormatter: new LokiJsonTextFormatter())
            .CreateLogger();
        tempLogger.Fatal(ex, "[AUTH TEMPORARY FATAL LOGGER] Application startup failed");
    }
    catch (Exception lokiEx)
    {
        Log.Warning(lokiEx, "Failed to send log to Loki");
    }
}
finally
{
    Log.CloseAndFlush();
}