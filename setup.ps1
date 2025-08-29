# Active Directory REST API Setup Script
# This script helps set up the development environment

param(
    [switch]$SkipDatabase,
    [switch]$SkipBuild,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Active Directory REST API Setup Script

Usage: .\setup.ps1 [options]

Options:
    -SkipDatabase    Skip starting the PostgreSQL database
    -SkipBuild       Skip building the .NET application
    -Help           Show this help message

Examples:
    .\setup.ps1                    # Full setup
    .\setup.ps1 -SkipDatabase     # Skip database, just build and run
    .\setup.ps1 -SkipBuild        # Skip build, just start database and run
"@
    exit 0
}

Write-Host "Active Directory REST API Setup" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET 8.0
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -notlike "8.*") {
        Write-Host "ERROR: .NET 8.0 is required. Current version: $dotnetVersion" -ForegroundColor Red
        Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ .NET $dotnetVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}

# Check Docker
try {
    $dockerVersion = docker --version
    Write-Host "✓ $dockerVersion found" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Docker not found. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Start PostgreSQL database
if (-not $SkipDatabase) {
    Write-Host "Starting PostgreSQL database..." -ForegroundColor Yellow
    
    try {
        # Stop any existing containers
        docker-compose down 2>$null
        
        # Start the database
        docker-compose up -d
        
        # Wait for database to be ready
        Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        # Check if database is running
        $containerStatus = docker-compose ps --format json | ConvertFrom-Json
        if ($containerStatus.State -eq "running") {
            Write-Host "✓ PostgreSQL database is running" -ForegroundColor Green
        } else {
            Write-Host "WARNING: Database container status: $($containerStatus.State)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "ERROR: Failed to start PostgreSQL database" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Skipping database setup..." -ForegroundColor Yellow
}

# Build the application
if (-not $SkipBuild) {
    Write-Host "Building the application..." -ForegroundColor Yellow
    
    try {
        dotnet restore
        dotnet build --configuration Release
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Application built successfully" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Build failed" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "ERROR: Failed to build application" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Skipping build..." -ForegroundColor Yellow
}

# Check configuration
Write-Host "Checking configuration..." -ForegroundColor Yellow

if (Test-Path "appsettings.json") {
    $config = Get-Content "appsettings.json" | ConvertFrom-Json
    
    if ($config.ActiveDirectory.Domain -eq "your-domain.com") {
        Write-Host "WARNING: Active Directory configuration not updated" -ForegroundColor Yellow
        Write-Host "Please update appsettings.json with your domain settings:" -ForegroundColor Yellow
        Write-Host "  - Domain" -ForegroundColor Yellow
        Write-Host "  - Container" -ForegroundColor Yellow
        Write-Host "  - Username" -ForegroundColor Yellow
        Write-Host "  - Password" -ForegroundColor Yellow
    } else {
        Write-Host "✓ Active Directory configuration found" -ForegroundColor Green
    }
} else {
    Write-Host "ERROR: appsettings.json not found" -ForegroundColor Red
    exit 1
}

# Create logs directory
if (-not (Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" | Out-Null
    Write-Host "✓ Created logs directory" -ForegroundColor Green
}

Write-Host ""
Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update appsettings.json with your Active Directory settings" -ForegroundColor Cyan
Write-Host "2. Run the application: dotnet run" -ForegroundColor Cyan
Write-Host "3. Access Swagger UI: https://localhost:7001" -ForegroundColor Cyan
Write-Host "4. Health check: https://localhost:7001/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database connection:" -ForegroundColor Cyan
Write-Host "  Host: localhost" -ForegroundColor Cyan
Write-Host "  Port: 5432" -ForegroundColor Cyan
Write-Host "  Database: active_directory_api" -ForegroundColor Cyan
Write-Host "  Username: ad_api_user" -ForegroundColor Cyan
Write-Host "  Password: ad_api_password" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop the database: docker-compose down" -ForegroundColor Cyan
