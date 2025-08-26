using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var adConfig = builder.Configuration.GetSection("ActiveDirectory").Get<ActiveDirectoryConfiguration>();
if (adConfig == null)
{
    throw new InvalidOperationException("Active Directory configuration is missing from appsettings.json");
}

builder.Services.AddSingleton(adConfig);

if (OperatingSystem.IsWindows())
{
    builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
}
else
{
    throw new PlatformNotSupportedException("Active Directory service is only supported on Windows.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var adService = scope.ServiceProvider.GetRequiredService<IActiveDirectoryService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting Active Directory connection validation...");

    try
    {
        var connectionValid = await adService.ValidateConnectionAsync();
        if (!connectionValid)
        {
            logger.LogCritical("Failed to establish Active Directory connection. Application cannot start.");
            throw new InvalidOperationException("Active Directory connection validation failed. Check your configuration and network connectivity.");
        }
        
        logger.LogInformation("Active Directory connection validated successfully. Starting application...");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Critical error during Active Directory connection validation. Application will not start.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
