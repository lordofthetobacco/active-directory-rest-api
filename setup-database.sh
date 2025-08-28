#!/bin/bash

# Setup script for PostgreSQL database and Entity Framework migrations
# This script helps set up the audit logging database for the Active Directory REST API

echo "🚀 Setting up PostgreSQL database for Active Directory REST API audit logging..."

# Check if PostgreSQL is running
if ! pg_isready -h localhost -p 5432 > /dev/null 2>&1; then
    echo "❌ PostgreSQL is not running on localhost:5432"
    echo "Please start PostgreSQL and try again"
    exit 1
fi

echo "✅ PostgreSQL is running"

# Check if database exists
if psql -h localhost -U postgres -lqt | cut -d \| -f 1 | grep -qw ad_audit_logs; then
    echo "✅ Database 'ad_audit_logs' already exists"
else
    echo "📝 Creating database 'ad_audit_logs'..."
    psql -h localhost -U postgres -c "CREATE DATABASE ad_audit_logs;"
    if [ $? -eq 0 ]; then
        echo "✅ Database created successfully"
    else
        echo "❌ Failed to create database"
        exit 1
    fi
fi

# Apply Entity Framework migrations
echo "🔄 Applying Entity Framework migrations..."
export PATH="$PATH:/Users/franek/.dotnet/tools"
dotnet ef database update --context ApplicationDbContext

if [ $? -eq 0 ]; then
    echo "✅ Database migrations applied successfully"
    echo ""
    echo "🎉 Database setup complete!"
    echo ""
    echo "Next steps:"
    echo "1. Update the connection string in appsettings.Development.json with your actual PostgreSQL credentials"
    echo "2. Update the Active Directory configuration in appsettings.Development.json"
    echo "3. Update the JWT Bearer configuration with your actual Entra ID tenant and application details"
    echo "4. Run the application with: dotnet run"
else
    echo "❌ Failed to apply database migrations"
    exit 1
fi
