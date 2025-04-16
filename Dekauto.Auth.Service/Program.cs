using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Dekauto.Auth.Service.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// ѕрименение конфигов.
builder.Configuration
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);

var connectionString = builder.Configuration.GetConnectionString("Main");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IUserAuthService, UserAuthService>();
builder.Services.AddTransient<IUsersRepository, UsersRepository>();
builder.Services.AddTransient<IRolesRepository, RolesRepository>();
builder.Services.AddTransient<IRolesService, RolesService>();
builder.Services.AddDbContext<DekautoContext>(options =>
    options.UseNpgsql(connectionString)
    .UseLazyLoadingProxies());
builder.Services.AddCors(options => options.AddPolicy("AngularLocalhost", policy =>
{
    policy.WithOrigins("http://localhost:4200") // јдрес Angular-приложени€
             .AllowAnyHeader()
             .AllowAnyMethod()
             .WithExposedHeaders("Content-Disposition")
             .AllowCredentials();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.

// явно указываем порты (дл€ Docker)
app.Urls.Add("http://*:5507");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AngularLocalhost");

}
else
{
    app.Urls.Add("https://*:5508");
    app.UseHttpsRedirection(); // без https редиректа в dev-версии
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
