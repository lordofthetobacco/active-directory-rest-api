# Active Directory REST API

A secure REST API for managing Active Directory users, groups, and organizational units. The API is secured with API keys and logs all operations to a PostgreSQL database.

## Features

- **User Management**: Create, read, update, delete, and manage Active Directory users
- **Group Management**: Manage groups and group memberships
- **Organizational Unit Management**: Handle OUs and their structure
- **API Key Authentication**: Secure all endpoints with configurable API keys
- **PostgreSQL Logging**: Log all API operations to a PostgreSQL database
- **Docker Support**: PostgreSQL database runs in Docker for easy setup

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- Active Directory domain access

## Configuration

### 1. Update appsettings.json

Update the `appsettings.json` file with your Active Directory configuration:

```json
{
  "ActiveDirectory": {
    "Domain": "your-domain.com",
    "Username": "admin@your-domain.com",
    "Password": "your-password",
    "SearchBase": "DC=your-domain,DC=com",
    "Server": "your-domain-controller.com"
  },
  "ApiKeys": {
    "ValidKeys": [
      "your-api-key-1",
      "your-api-key-2"
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ad_logs;Username=postgres;Password=postgres"
  }
}
```

### 2. API Keys

Add your API keys to the `ApiKeys:ValidKeys` array in `appsettings.json`. These keys will be used to authenticate API requests.

## Running the Application

### 1. Start PostgreSQL Database

Start only the PostgreSQL database in Docker:

```bash
docker-compose up postgres -d
```

This will:
- Start PostgreSQL on port 5432
- Create the `ad_logs` database
- Set up the required tables automatically

### 2. Run the API Locally

1. **Install dependencies:**
   ```bash
   dotnet restore
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access the API:**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Swagger UI: http://localhost:5000/swagger

### 3. Stop the Database

When you're done, stop the PostgreSQL container:

```bash
docker-compose down
```

## API Endpoints

### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/user` | Get all users |
| GET | `/user/queryable?attributes=name,email,displayName` | Get all users with specific attributes |
| POST | `/user` | Create a new user |
| GET | `/user/{username}` | Get a specific user |
| GET | `/user/{username}/queryable?attributes=name,email,displayName` | Get specific user with specific attributes |
| PUT | `/user/{username}` | Update a user |
| GET | `/user/{username}/exists` | Check if user exists |
| GET | `/user/{username}/member-of/{groupName}` | Check group membership |
| POST | `/user/{username}/authenticate` | Authenticate user |
| PUT | `/user/{username}/password` | Change password |
| PUT | `/user/{username}/password-never-expires` | Set password never expires |
| PUT | `/user/{username}/password-expires` | Set password expires |
| PUT | `/user/{username}/enable` | Enable/disable user |
| PUT | `/user/{username}/move` | Move user to different OU |
| PUT | `/user/{username}/unlock` | Unlock user account |
| DELETE | `/user/{username}` | Disable user (soft delete) |

### Groups

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/group` | Get all groups |
| GET | `/group/queryable?attributes=name,description,member` | Get all groups with specific attributes |
| POST | `/group` | Create a new group |
| GET | `/group/{groupName}` | Get a specific group |
| GET | `/group/{groupName}/queryable?attributes=name,description,member` | Get specific group with specific attributes |
| GET | `/group/{groupName}/exists` | Check if group exists |
| POST | `/group/{groupName}/user/{username}` | Add user to group |
| DELETE | `/group/{groupName}/user/{username}` | Remove user from group |

### Organizational Units

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/ou` | Get all OUs |
| GET | `/ou/queryable?attributes=name,description` | Get all OUs with specific attributes |
| POST | `/ou` | Create a new OU |
| GET | `/ou/{ouName}` | Get a specific OU |
| GET | `/ou/{ouName}/queryable?attributes=name,description` | Get specific OU with specific attributes |
| GET | `/ou/{ouName}/exists` | Check if OU exists |

### Other

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/other` | Other operations endpoint |
| GET | `/all` | Get all AD objects |
| GET | `/find/users` | Search for users with filters |
| GET | `/find/groups` | Search for groups with filters |
| GET | `/find/custom` | Search with custom LDAP filter |
| GET | `/status` | Get AD connection status |

### Search Endpoints

#### Find Users
```bash
# Search by name (partial match)
GET /find/users?name=john

# Search by email (partial match)
GET /find/users?email=john@domain.com

# Search by organizational unit
GET /find/users?ou=IT

# Combine multiple filters
GET /find/users?name=john&email=john@domain.com&ou=IT
```

#### Find Groups
```bash
# Search by name (partial match)
GET /find/groups?name=admin

# Search by description (partial match)
GET /find/groups?description=administrators

# Search by organizational unit
GET /find/groups?ou=IT

# Combine multiple filters
GET /find/groups?name=admin&description=administrators&ou=IT
```

#### Custom Search
```bash
# Use custom LDAP filter
GET /find/custom?filter=(&(objectClass=user)(mail=*@domain.com))
```

### Queryable Endpoints

The API now provides queryable endpoints that allow you to specify which Active Directory attributes to retrieve. This gives you full control over the data returned and can improve performance by only fetching the attributes you need.

#### Usage Examples

**Get all users with specific attributes:**
```bash
# Get only name and email
GET /user/queryable?attributes=name,email

# Get name, email, and display name
GET /user/queryable?attributes=name,email,displayName

# Get all attributes (default behavior)
GET /user/queryable
```

**Get specific user with specific attributes:**
```bash
# Get user with only name and email
GET /user/john/queryable?attributes=name,email

# Get user with name, email, and group memberships
GET /user/john/queryable?attributes=name,email,memberOf
```

**Get groups with specific attributes:**
```bash
# Get groups with name and description
GET /group/queryable?attributes=name,description

# Get groups with name, description, and members
GET /group/queryable?attributes=name,description,member
```

**Get organizational units with specific attributes:**
```bash
# Get OUs with name and description
GET /ou/queryable?attributes=name,description
```

#### Available Attributes

Common Active Directory attributes you can request include:
- **Users**: `name`, `sAMAccountName`, `displayName`, `mail`, `userPrincipalName`, `enabled`, `lastLogon`, `memberOf`, `distinguishedName`
- **Groups**: `name`, `sAMAccountName`, `description`, `member`, `distinguishedName`
- **Organizational Units**: `name`, `description`, `distinguishedName`

#### Response Format

Queryable endpoints return data in the following format:
```json
{
  "success": true,
  "message": "Users retrieved successfully with queryable attributes",
  "data": [
    {
      "distinguishedName": "CN=John Doe,CN=Users,DC=domain,DC=com",
      "objectClass": "user",
      "attributes": {
        "name": "John Doe",
        "sAMAccountName": "johndoe",
        "mail": "john.doe@domain.com",
        "displayName": "John Doe"
      }
    }
  ]
}
```

## Authentication

All API endpoints require authentication using an API key. Include the API key in the `X-API-Key` header:

```bash
curl -H "X-API-Key: your-api-key-1" \
     -H "Content-Type: application/json" \
     http://localhost:5000/user
```

## API Key Management

The API now uses a PostgreSQL database to store and manage API keys instead of configuration files. This provides:

- **Dynamic Key Management**: Add/remove API keys without restarting the application
- **Key Tracking**: Monitor when keys were created and last used
- **Key Status**: Activate/deactivate keys as needed
- **Audit Trail**: Track who created each key and when

### Managing API Keys

A separate Blazor Server application (`ApiKeyManager`) is provided for managing API keys:

1. **Secure Interface**: Uses Entra ID (Azure AD) for authentication
2. **Full CRUD Operations**: Create, read, update, delete API keys
3. **Key Generation**: Automatically generates secure 32-character keys
4. **Status Management**: Activate/deactivate keys as needed

### Running the API Key Manager

```bash
cd ApiKeyManager
dotnet restore
dotnet run
```

Access the management interface at `https://localhost:7000` after configuring Entra ID authentication.

See the `ApiKeyManager/README.md` for detailed setup and configuration instructions.

## Database Schema

The API automatically creates a PostgreSQL database with the following tables:

### ApiLogs

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Timestamp | timestamp | When the API call was made |
| Endpoint | varchar(200) | The endpoint that was called |
| Method | varchar(10) | HTTP method used |
| ApiKey | varchar(100) | API key used (nullable) |
| StatusCode | int | HTTP status code returned |
| ResponseTime | bigint | Response time in milliseconds |
| ErrorMessage | text | Error message if any (nullable) |

### ApiKeys

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Key | varchar(100) | Unique API key (32 characters) |
| Name | varchar(100) | Human-readable name |
| Description | varchar(500) | Optional description |
| IsActive | boolean | Whether the key is active |
| CreatedAt | timestamp | When the key was created |
| LastUsedAt | timestamp | When the key was last used |
| CreatedBy | varchar(100) | User who created the key |

## Development

### Project Structure

```
├── Controllers/           # API controllers
├── Models/               # DTOs and configuration models
├── Services/             # Business logic services
├── Data/                 # Entity Framework context
├── Authentication/       # API key authentication
├── Middleware/           # API logging middleware
├── docker-compose.yml    # PostgreSQL database only
└── appsettings.json     # Configuration
```

### Adding New Endpoints

1. Add the method to `IActiveDirectoryService`
2. Implement the method in `ActiveDirectoryService`
3. Create the controller endpoint
4. Update the `endpoints.md` file

### Testing

The API includes Swagger UI for testing endpoints. Access it at `/swagger` when running the application.

## Security Considerations

- **API Keys**: Store API keys securely and rotate them regularly
- **Network Security**: Use HTTPS in production
- **Active Directory**: Use a dedicated service account with minimal required permissions
- **Database**: Secure the PostgreSQL connection and use strong passwords

## Troubleshooting

### Common Issues

1. **Active Directory Connection Failed**
   - Verify domain credentials in `appsettings.json`
   - Check network connectivity to domain controller
   - Ensure service account has required permissions

2. **Database Connection Failed**
   - Verify PostgreSQL is running: `docker-compose ps`
   - Check connection string in `appsettings.json` (should be `Host=localhost`)
   - Ensure database exists: `docker-compose exec postgres psql -U postgres -d ad_logs`

3. **API Key Authentication Failed**
   - Verify API key is included in `X-API-Key` header
   - Check API key is in the `ApiKeys:ValidKeys` array
   - Ensure no extra spaces or characters in the key

### Logs

Check the application logs for detailed error information:

```bash
# PostgreSQL logs
docker-compose logs postgres

# API logs (in your terminal where dotnet run is executed)
```

## License

This project is licensed under the MIT License.
