# Active Directory REST API

A RESTful API for interacting with Microsoft Active Directory, built with ASP.NET Core 8.0.

## Features

- **User Management**: Create, read, update, delete, enable/disable users
- **Group Management**: Search and retrieve group information
- **Authentication**: Validate user credentials
- **Search**: Advanced search capabilities for users and groups
- **Password Management**: Reset passwords and unlock accounts
- **Health Monitoring**: API health checks and AD connection status

## Prerequisites

- .NET 8.0 SDK
- Access to an Active Directory domain
- Service account with appropriate permissions for AD operations

## Configuration

Update `appsettings.json` with your Active Directory settings:

```json
{
  "ActiveDirectory": {
    "Server": "your-domain-controller.com",
    "Port": 389,
    "UseSSL": false,
    "BindDN": "CN=ServiceAccount,OU=ServiceAccounts,DC=yourdomain,DC=com",
    "BindPassword": "your-service-account-password",
    "SearchBase": "DC=yourdomain,DC=com",
    "Timeout": 30,
    "UseIntegratedSecurity": false
  }
}
```

### Configuration Options

- **Server**: Domain controller hostname or IP address
- **Port**: LDAP port (389 for non-SSL, 636 for SSL)
- **UseSSL**: Enable SSL/TLS encryption
- **BindDN**: Distinguished name of the service account
- **BindPassword**: Service account password
- **SearchBase**: Base DN for searches
- **Timeout**: Connection timeout in seconds
- **UseIntegratedSecurity**: Use Windows authentication (requires domain-joined machine)

## API Endpoints

### Users

#### GET `/api/users/{samAccountName}`
Retrieve user by SAM account name.

#### GET `/api/users/email/{email}`
Retrieve user by email address.

#### GET `/api/users/dn/{distinguishedName}`
Retrieve user by distinguished name.

#### POST `/api/users/search`
Search users with filters and pagination.

**Request Body:**
```json
{
  "searchTerm": "john",
  "maxResults": 50,
  "attributes": ["displayName", "email", "department"]
}
```

#### POST `/api/users/authenticate`
Authenticate user credentials.

**Request Body:**
```json
{
  "username": "john.doe",
  "password": "password123"
}
```

#### GET `/api/users/{username}/groups`
Get all groups for a user.

#### GET `/api/users/{username}/groups/{groupName}/ismember`
Check if user is member of specific group.

#### POST `/api/users`
Create a new user.

**Request Body:**
```json
{
  "user": {
    "samAccountName": "john.doe",
    "displayName": "John Doe",
    "givenName": "John",
    "surname": "Doe",
    "email": "john.doe@company.com",
    "enabled": true
  },
  "password": "SecurePassword123!"
}
```

#### PUT `/api/users/{samAccountName}`
Update user information.

#### DELETE `/api/users/{samAccountName}`
Delete a user.

#### POST `/api/users/{samAccountName}/enable`
Enable a user account.

#### POST `/api/users/{samAccountName}/disable`
Disable a user account.

#### POST `/api/users/{samAccountName}/reset-password`
Reset user password.

**Request Body:**
```json
{
  "newPassword": "NewSecurePassword123!"
}
```

#### POST `/api/users/{samAccountName}/unlock`
Unlock a locked user account.

### Groups

#### GET `/api/groups/{groupName}`
Retrieve group information.

#### POST `/api/groups/search`
Search groups with filters.

### Health

#### GET `/api/health`
Overall API health status.

#### GET `/api/health/ad`
Active Directory connection health.

## Running the API

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI:**
   Navigate to `https://localhost:5001/swagger` for interactive API documentation.

## Security Considerations

- Store service account credentials securely
- Use SSL/TLS in production environments
- Implement proper authentication and authorization for API access
- Consider using Windows Authentication for domain-joined machines
- Regularly rotate service account passwords
- Limit service account permissions to minimum required

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK`: Successful operation
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Logging

All operations are logged with appropriate log levels. Check application logs for detailed error information and debugging.

## Dependencies

- **System.DirectoryServices.Protocols**: Core LDAP functionality
- **System.DirectoryServices.AccountManagement**: High-level AD operations
- **Microsoft.AspNetCore.OpenApi**: Swagger/OpenAPI support
- **Swashbuckle.AspNetCore**: API documentation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.
