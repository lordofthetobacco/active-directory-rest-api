# PostgreSQL Database Logging Setup Guide

This guide explains how to set up and use the PostgreSQL database logging system for the Active Directory REST API.

## 🏗️ Architecture Overview

The database logging system consists of several components:

1. **AuditLog Model** - Entity Framework Core model for storing audit logs
2. **ApplicationDbContext** - Database context for Entity Framework operations
3. **IDatabaseLoggingService** - Interface defining database logging operations
4. **PostgresLoggingService** - Implementation for PostgreSQL database logging
5. **AuditLoggingService** - Enhanced service that logs to both file and database

## 📋 Prerequisites

- PostgreSQL 12+ installed and running
- .NET 8.0 SDK
- Entity Framework Core tools (`dotnet-ef`)

## 🚀 Quick Setup

### 1. Install Entity Framework Tools

```bash
dotnet tool install --global dotnet-ef
```

### 2. Configure Database Connection

Update `appsettings.Development.json` with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ad_audit_logs;Username=your_username;Password=your_password;Port=5432"
  }
}
```

### 3. Run Database Setup Script

```bash
./setup-database.sh
```

This script will:
- Check PostgreSQL connectivity
- Create the `ad_audit_logs` database
- Apply Entity Framework migrations
- Create the `audit_logs` table with proper indexes

## 🗄️ Database Schema

The `audit_logs` table includes the following fields:

| Field | Type | Description |
|-------|------|-------------|
| `id` | BIGINT | Primary key, auto-increment |
| `timestamp` | TIMESTAMP | When the event occurred (UTC) |
| `correlation_id` | VARCHAR(50) | Unique identifier linking related log entries |
| `log_type` | VARCHAR(50) | Type of log entry (API_REQUEST, API_RESPONSE, etc.) |
| `action` | VARCHAR(100) | Action performed |
| `resource` | VARCHAR(200) | Resource affected |
| `user_context` | VARCHAR(500) | User/application context |
| `request_data` | JSONB | Request payload (masked for sensitive data) |
| `response_data` | JSONB | Response payload |
| `status_code` | INTEGER | HTTP status code |
| `duration_ms` | DOUBLE PRECISION | Operation duration in milliseconds |
| `error_message` | VARCHAR(1000) | Error description |
| `exception_details` | TEXT | Full exception details |
| `ip_address` | VARCHAR(45) | Client IP address |
| `user_agent` | VARCHAR(500) | Client user agent |
| `http_method` | VARCHAR(10) | HTTP method used |
| `endpoint` | VARCHAR(200) | API endpoint called |
| `ad_operation` | VARCHAR(100) | Active Directory operation |
| `ad_target` | VARCHAR(200) | Active Directory target |
| `ad_success` | BOOLEAN | Whether AD operation succeeded |
| `created_at` | TIMESTAMP | Record creation time (UTC) |
| `updated_at` | TIMESTAMP | Record last update time (UTC) |

## 🔍 Database Indexes

The following indexes are automatically created for optimal query performance:

- **Primary indexes**: `timestamp`, `correlation_id`, `log_type`, `action`, `resource`
- **Composite indexes**: `(timestamp, log_type)`, `(correlation_id, timestamp)`, `(action, timestamp)`
- **Additional indexes**: `user_context`, `status_code`, `created_at`

## 📊 Log Types

The system logs the following types of events:

- **API_REQUEST** - Incoming API requests
- **API_RESPONSE** - API responses (successful)
- **API_ERROR** - API errors and exceptions
- **AUTH_SUCCESS** - Successful authentication events
- **AUTH_FAILURE** - Failed authentication attempts
- **AD_OPERATION** - Active Directory operations

## 🛠️ Usage Examples

### Basic Logging

The system automatically logs all API interactions. No additional code is required in your controllers.

### Querying Logs

```csharp
// Get logs by correlation ID
var logs = await _databaseLogger.GetLogsByCorrelationIdAsync(correlationId);

// Get logs by time range
var startTime = DateTime.UtcNow.AddDays(-1);
var endTime = DateTime.UtcNow;
var logs = await _databaseLogger.GetLogsByTimeRangeAsync(startTime, endTime);

// Get logs by action
var logs = await _databaseLogger.GetLogsByActionAsync("GetUser", limit: 100);

// Get error logs
var errorLogs = await _databaseLogger.GetErrorLogsAsync(startTime, endTime, limit: 100);
```

### Maintenance Operations

```csharp
// Clean up old logs (older than 90 days)
var cutoffDate = DateTime.UtcNow.AddDays(-90);
await _databaseLogger.CleanupOldLogsAsync(cutoffDate);

// Optimize table performance
await _databaseLogger.OptimizeTableAsync();
```

## 🔒 Security Features

### Sensitive Data Masking

The system automatically masks sensitive information in logs:

- **Password fields** are replaced with `[REDACTED]`
- **Client secrets** are masked
- **Tokens** are truncated
- **Personal information** can be configured for additional masking

### Access Control

- Logs are stored in a separate database
- Database access is restricted to the application service account
- No sensitive data is exposed in log queries

## 📈 Performance Considerations

### Fallback Logging

If database logging fails, the system automatically falls back to file logging to ensure no audit events are lost.

### Async Operations

All database logging operations are performed asynchronously to avoid blocking API responses.

### Connection Pooling

Entity Framework Core automatically manages database connections for optimal performance.

## 🧪 Testing

### Test Database Connectivity

```bash
./test-database-logging.ps1
```

### Manual Testing

1. Start the application: `dotnet run`
2. Make API calls to generate logs
3. Check the database for new log entries
4. Verify correlation IDs link related operations

## 🚨 Troubleshooting

### Common Issues

1. **Connection Failed**: Verify PostgreSQL is running and credentials are correct
2. **Migration Errors**: Ensure Entity Framework tools are installed and PATH is set
3. **Permission Denied**: Check database user permissions for the `ad_audit_logs` database

### Debug Mode

Enable detailed logging for the database service:

```json
{
  "Logging": {
    "LogLevel": {
      "ActiveDirectory_API.Services.PostgresLoggingService": "Debug"
    }
  }
}
```

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

## 🔄 Migration and Updates

### Adding New Fields

1. Update the `AuditLog` model
2. Create a new migration: `dotnet ef migrations add AddNewField`
3. Apply the migration: `dotnet ef database update`

### Schema Changes

Always test schema changes in a development environment before applying to production.

---

**Note**: This logging system is designed for audit and compliance purposes. Ensure your organization's data retention and privacy policies are followed when configuring log cleanup and access controls.
