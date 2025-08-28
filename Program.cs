using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using ActiveDirectory_API.Data;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Logging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HTTP context accessor for correlation ID tracking
builder.Services.AddHttpContextAccessor();

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register database logging service
builder.Services.AddScoped<IDatabaseLoggingService, PostgresLoggingService>();

// Register audit logging service
builder.Services.AddScoped<IAuditLoggingService, AuditLoggingService>();

// Configure JWT Bearer Authentication for Machine-to-Machine communication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["JwtBearer:Authority"];
        options.Audience = builder.Configuration["JwtBearer:Audience"];
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        
        // Configure token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = builder.Configuration.GetValue<bool>("JwtBearer:ValidateIssuer"),
            ValidateAudience = builder.Configuration.GetValue<bool>("JwtBearer:ValidateAudience"),
            ValidateLifetime = builder.Configuration.GetValue<bool>("JwtBearer:ValidateLifetime"),
            ValidateIssuerSigningKey = builder.Configuration.GetValue<bool>("JwtBearer:ValidateIssuerSigningKey"),
            ClockSkew = TimeSpan.Zero
        };

        // Handle authentication events for logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Note: We can't access audit logger here during configuration
                // Authentication events will be logged by the middleware
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Note: We can't access audit logger here during configuration
                // Authentication events will be logged by the middleware
                return Task.CompletedTask;
            }
        };
    });

// Enable detailed logging for JWT validation in development
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

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
    
    // Policy for admin operations - requires specific application permissions
    options.AddPolicy("RequireAdminPermissions", policy =>
        policy.RequireClaim("roles", "Directory.ReadWrite.All", "User.ReadWrite.All", "Group.ReadWrite.All"));
    
    // Policy for read-only operations
    options.AddPolicy("RequireReadPermissions", policy =>
        policy.RequireClaim("roles", "Directory.Read.All", "User.Read.All", "Group.Read.All", "Directory.ReadWrite.All", "User.ReadWrite.All", "Group.ReadWrite.All"));
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

// Add correlation ID middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    await next();
});

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
