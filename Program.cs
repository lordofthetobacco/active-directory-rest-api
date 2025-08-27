using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("GraphAPI"));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Active Directory API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                }
            },
            Array.Empty<string>()
        }
    });
});

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

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
    
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin", "Global Administrator"));
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

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
