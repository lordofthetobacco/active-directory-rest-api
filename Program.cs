using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using active_directory_rest_api.Authentication;
using active_directory_rest_api.Data;
using active_directory_rest_api.Services;
using active_directory_rest_api.Middleware;
using active_directory_rest_api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<ActiveDirectoryConfig>(
    builder.Configuration.GetSection("ActiveDirectory"));
builder.Services.Configure<LoggingDbConfig>(
    builder.Configuration.GetSection("LoggingDb"));

// Database Context
builder.Services.AddDbContext<LoggingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

builder.Services.AddAuthorization();

if (OperatingSystem.IsWindows())
{
    builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
} else {
    throw new PlatformNotSupportedException("Active Directory is only supported on Windows");
}
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add API logging middleware
app.UseMiddleware<ApiLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
