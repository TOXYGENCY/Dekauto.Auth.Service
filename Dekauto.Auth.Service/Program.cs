using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Dekauto.Auth.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// ���������� ��������.
builder.Configuration
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);

var connectionString = builder.Configuration.GetConnectionString("Main");

// ��������� JWT �������
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

// �������� ������� � ����������
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("OnlyAdmin", policy => policy.RequireRole("�������������"));

builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// ���������� swagger � ������������
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "������� JWT ����� (��� ����� 'Bearer')",
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
builder.Services.AddDbContext<DekautoContext>(options =>
    options.UseNpgsql(connectionString)
    .UseLazyLoadingProxies());
builder.Services.AddCors(options => options.AddPolicy("AngularLocalhost", policy =>
{
    policy.WithOrigins("http://localhost:4200") // ����� Angular-����������
             .AllowAnyHeader()
             .AllowAnyMethod()
             .WithExposedHeaders("Content-Disposition")
             .AllowCredentials();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.

// ���� ��������� ����� (��� Docker)
app.Urls.Add("http://*:5507");

// 1. �������������� (JWT, ���� � �.�.)
app.UseAuthentication();

// 2. ����������� (�������� ��������� [Authorize])
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AngularLocalhost");

}
else
{
    app.Urls.Add("https://*:5508");
    app.UseHttpsRedirection(); // ��� https ��������� � dev-������
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
