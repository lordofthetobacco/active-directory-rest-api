# Active Directory REST API

A comprehensive REST API for managing Active Directory users, groups, and organizational units with robust authentication, authorization, and audit logging.

## Features

- **Complete Active Directory Management**: Full CRUD operations for users, groups, and organizational units
- **Secure API Token Authentication**: Role-based access control with scoped permissions
- **Comprehensive Audit Logging**: All API calls are logged to PostgreSQL with detailed information
- **PostgreSQL Database**: Robust data storage with proper indexing and relationships
- **Docker Support**: PostgreSQL runs in Docker for easy development setup
- **Windows Native**: API runs natively on Windows for optimal Active Directory integration
- **Swagger Documentation**: Interactive API documentation with authentication support

## Prerequisites

- Windows 10/11 or Windows Server 2016+
- .NET 8.0 SDK
- Docker Desktop (for PostgreSQL)
- Active Directory domain access
- Service account with appropriate permissions

## Quick Start

### 1. Clone and Setup

```bash
git clone <repository-url>
cd active-directory-rest-api
```

### 2. Configure Active Directory Settings

Edit `appsettings.json` and update the Active Directory configuration:

```json
{
  "ActiveDirectory": {
    "Domain": "your-domain.com",
    "Container": "DC=your-domain,DC=com",
    "Username": "service-account@your-domain.com",
    "Password": "your-service-account-password"
  }
}
```

### 3. Start PostgreSQL Database

```bash
docker-compose up -d
```

This will start PostgreSQL on port 5432 with the following credentials:
- Database: `active_directory_api`
- Username: `ad_api_user`
- Password: `ad_api_password`

### 4. Run the API

```bash
dotnet run
```

The API will be available at:
- API: https://localhost:7001
- Swagger UI: https://localhost:7001
- Health Check: https://localhost:7001/health

## API Endpoints

### Users
- `GET /user` - Get all users
- `POST /user` - Create a new user
- `GET /user/{user}` - Get a specific user
- `PUT /user/{user}` - Update a user
- `GET /user/{user}/exists` - Check if user exists
- `GET /user/{user}/member-of/{group}` - Check group membership
- `POST /user/{user}/authenticate` - Authenticate user
- `PUT /user/{user}/password` - Change password
- `PUT /user/{user}/password-never-expires` - Set password to never expire
- `PUT /user/{user}/password-expires` - Set password to expire
- `PUT /user/{user}/enable` - Enable user
- `PUT /user/{user}/disable` - Disable user
- `PUT /user/{user}/move` - Move user to different OU
- `PUT /user/{user}/unlock` - Unlock user account
- `DELETE /user/{user}` - Delete user

### Groups
- `GET /group` - Get all groups
- `POST /group` - Create a new group
- `GET /group/{group}` - Get a specific group
- `GET /group/{group}/exists` - Check if group exists
- `POST /group/{group}/user/{user}` - Add user to group
- `DELETE /group/{group}/user/{user}` - Remove user from group
- `DELETE /group/{group}` - Delete group

### Organizational Units
- `GET /ou` - Get all OUs
- `POST /ou` - Create a new OU
- `GET /ou/{ou}` - Get a specific OU
- `GET /ou/{ou}/exists` - Check if OU exists
- `DELETE /ou/{ou}` - Delete OU

### Other
- `GET /other` - Get other AD information
- `GET /all` - Get summary of all AD objects
- `GET /find/{filter}` - Find items using custom filter
- `GET /status` - Get AD connection status

### API Token Management
- `GET /api-tokens` - Get all API tokens
- `GET /api-tokens/{id}` - Get specific API token
- `POST /api-tokens` - Create new API token
- `DELETE /api-tokens/{id}` - Revoke API token

## Authentication

The API uses API tokens for authentication. Include the token in one of these ways:

### Option 1: Authorization Header
```
Authorization: Bearer your-api-token-here
```

### Option 2: X-API-Key Header
```
X-API-Key: your-api-token-here
```

## API Token Scopes

Tokens can have the following scopes:

- `users:read` - Read user information
- `users:write` - Create/update users
- `users:delete` - Delete users
- `groups:read` - Read group information
- `groups:write` - Create/update groups
- `groups:delete` - Delete groups
- `ous:read` - Read OU information
- `ous:write` - Create/update OUs
- `ous:delete` - Delete OUs
- `other:read` - Read other AD information
- `all:read` - Read summary information
- `find:read` - Use find functionality
- `status:read` - Read status information
- `admin:read` - Read admin information
- `admin:write` - Write admin information

## Database Schema

### API Tokens Table
- `id` - Primary key
- `token_hash` - SHA256 hash of the token
- `name` - Token name/description
- `description` - Detailed description
- `scopes` - Array of permission scopes
- `is_active` - Whether token is active
- `created_at` - Creation timestamp
- `expires_at` - Expiration timestamp
- `last_used_at` - Last usage timestamp

### Audit Logs Table
- `id` - Primary key
- `timestamp` - Log timestamp
- `api_token_id` - Reference to API token
- `endpoint` - API endpoint called
- `method` - HTTP method used
- `user_agent` - Client user agent
- `ip_address` - Client IP address
- `request_body` - Request body content
- `response_status` - HTTP response status
- `response_body` - Response body content
- `execution_time_ms` - Request execution time
- `error_message` - Error message if any
- `additional_data` - JSON additional data

## Development

### Adding New Endpoints

1. Add the method to `IActiveDirectoryService`
2. Implement in `ActiveDirectoryService`
3. Create controller method with appropriate scope attributes
4. Add to the endpoints documentation

### Database Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings__DefaultConnection` - Database connection string
- `ActiveDirectory__Domain` - Active Directory domain
- `ActiveDirectory__Container` - Active Directory container
- `ActiveDirectory__Username` - Service account username
- `ActiveDirectory__Password` - Service account password

### Logging

The API uses Serilog with multiple sinks:
- Console output
- File logging (daily rotation)
- PostgreSQL database logging

## Security Considerations

- API tokens are hashed using SHA256 before storage
- All API calls are logged for audit purposes
- Scope-based authorization prevents unauthorized access
- HTTPS is enforced in production
- Service account credentials should be stored securely

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Ensure PostgreSQL is running: `docker-compose ps`
   - Check connection string in `appsettings.json`
   - Verify database credentials

2. **Active Directory Connection Failed**
   - Verify domain and container settings
   - Check service account credentials
   - Ensure network connectivity to domain controllers

3. **Authentication Failed**
   - Verify API token is valid and not expired
   - Check token has required scopes
   - Ensure token is included in request headers

### Logs

Check logs in:
- Console output
- `logs/api-*.log` files
- PostgreSQL `audit_logs` table

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the audit logs for debugging
