using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Dekauto.Auth.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


// Настройка логгера Serilog
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console(
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
.WriteTo.File("logs/Dekauto-Auth-.log", 
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", 
    rollingInterval: RollingInterval.Day,
    rollOnFileSizeLimit: true,
    fileSizeLimitBytes: 10485760, // Ограничение на размер одного лога 10 MB
    retainedFileCountLimit: 31, // может быть 31 файл с последними логами, перед тем, как они будут удаляться
    encoding: Encoding.UTF8) 
.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    // Применение конфигов.
    builder.Configuration
        .AddEnvironmentVariables()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
        .AddCommandLine(args);

    builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("Jwt__Key");
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        var mes = "Invalid secret key for JWT tokens - needs to be at least 32 characters long.";
        Log.Fatal(mes);
        throw new InvalidOperationException(mes);
    }

    var connectionString = builder.Configuration.GetConnectionString("Main");



    // Добавляем JWT сервисы
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
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

    // TODO: настроить связь между латинскими названиями 
    // Политики доступа к эндпоинтам
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("OnlyAdmin", policy => policy.RequireRole("Администратор"));

    builder.Services.AddAuthorization();
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
    builder.Services.AddTransient<IUserAuthService, UserAuthService>();
    builder.Services.AddTransient<IUsersRepository, UsersRepository>();
    builder.Services.AddTransient<IRolesRepository, RolesRepository>();
    builder.Services.AddTransient<IRolesService, RolesService>();
    builder.Services.AddSingleton<IRequestMetricsService, RequestMetricsService>();
    builder.Services.AddDbContext<DekautoContext>(options =>
        options.UseNpgsql(connectionString)
        .UseLazyLoadingProxies());
    builder.Services.AddCors(options => options.AddPolicy("AngularLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Адрес Angular-приложения
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .WithExposedHeaders("Content-Disposition")
                 .AllowCredentials();
    }));


    Log.Information("Building the application...");
    var app = builder.Build();

    // Configure the HTTP request pipeline.

    app.UseCors("AngularLocalhost");

    // Явно указываем порты (для Docker)
    app.Urls.Add("http://*:5507");

    if (app.Environment.IsDevelopment())
    {
        Log.Warning("The Development version of the application is started. Swagger activation...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Используем https, если это указано в конфиге
    if (Boolean.Parse(app.Configuration["UseHttps"]))
    {
        app.Urls.Add("https://*:5508");
        app.UseHttpsRedirection();
        Log.Information("Enabled HTTPS.");
    }

    // Аутентификация (JWT, куки и т.д.)
    app.UseAuthentication();

    // Авторизация (проверка атрибутов [Authorize])
    app.UseAuthorization();

    app.MapControllers();

    app.UseMetricsMiddleware(); // Метрики

    Log.Information("Application startup...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
}
finally
{
    Log.CloseAndFlush();
}