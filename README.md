# Active Directory REST API with Microsoft Entra ID Authentication

A secure REST API for managing Active Directory users and groups, protected with Microsoft Entra ID (formerly Azure AD) authentication.

## Features

- **Microsoft Entra ID Authentication**: Secure API access using JWT tokens
- **Role-Based Access Control**: Different permission levels for users and administrators
- **Active Directory Management**: Full CRUD operations for users and groups
- **Health Monitoring**: API health checks and Active Directory connectivity status
- **Swagger Documentation**: Interactive API documentation with authentication support

## Prerequisites

- .NET 8.0 SDK
- Windows operating system (for Active Directory services)
- Microsoft Entra ID tenant
- Registered application in Microsoft Entra ID

## Microsoft Entra ID Setup

### 1. Register Your Application

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Microsoft Entra ID** > **App registrations**
3. Click **New registration**
4. Enter application details:
   - **Name**: Active Directory API
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Web - `https://localhost:7001/signin-oidc` (for development)
5. Click **Register**

### 2. Configure Authentication

1. In your app registration, go to **Authentication**
2. Add platform configuration:
   - **Platform type**: Web
   - **Redirect URIs**: 
     - `https://localhost:7001/signin-oidc`
     - `https://localhost:7001/swagger/oauth2-redirect.html`
3. Under **Implicit grant and hybrid flows**, check:
   - **Access tokens**
   - **ID tokens**
4. Click **Save**

### 3. Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add description and select expiration
4. **Copy the secret value** (you won't see it again)

### 4. Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Application permissions**:
   - `User.ReadWrite.All`
   - `Group.ReadWrite.All`
   - `Directory.ReadWrite.All`
5. Click **Grant admin consent**

### 5. Get Application Details

Note down these values from your app registration:
- **Application (client) ID**
- **Directory (tenant) ID**
- **Client secret** (from step 3)

## Configuration

### 1. Update appsettings.Development.json

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  },
  "GraphAPI": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "https://graph.microsoft.com/.default"
  }
}
```

### 2. Production Configuration

For production, use environment variables or Azure Key Vault:

```bash
# Environment variables
AZUREAD__TENANTID=your-tenant-id
AZUREAD__CLIENTID=your-client-id
AZUREAD__CLIENTSECRET=your-client-secret
```

## API Endpoints

### Authentication Endpoints

- `GET /api/auth/me` - Get current user information
- `GET /api/auth/roles` - Get current user roles
- `POST /api/auth/logout` - Logout current user

### User Management (Requires Authentication)

- `GET /api/users/{samAccountName}` - Get user by SAM account name
- `GET /api/users/email/{email}` - Get user by email
- `POST /api/users/search` - Search users
- `GET /api/users/{username}/groups` - Get user groups

### User Management (Requires Admin Role)

- `POST /api/users` - Create new user
- `PUT /api/users/{samAccountName}` - Update user
- `POST /api/users/{samAccountName}/enable` - Enable user
- `POST /api/users/{samAccountName}/disable` - Disable user
- `POST /api/users/{samAccountName}/reset-password` - Reset user password
- `DELETE /api/users/{samAccountName}` - Delete user

### Group Management (Requires Authentication)

- `GET /api/groups/{groupName}` - Get group information
- `POST /api/groups/search` - Search groups
- `GET /api/groups/{groupName}/members/{username}/check` - Check if user is in group

### Group Management (Requires Admin Role)

- `POST /api/groups/{groupName}/members/{username}` - Add user to group
- `DELETE /api/groups/{groupName}/members/{username}` - Remove user from group

### Health Check (No Authentication Required)

- `GET /api/health` - API health status
- `GET /api/health/ad` - Active Directory connectivity status

## Authentication Flow

1. **Client obtains access token** from Microsoft Entra ID
2. **Include token in requests** using Authorization header:
   ```
   Authorization: Bearer <access_token>
   ```
3. **API validates token** and extracts user claims
4. **Authorization policies** check user roles for protected endpoints

## Role-Based Access Control

- **Authenticated Users**: Can read user/group information and perform searches
- **Admin Users**: Can perform all operations including user/group creation, modification, and deletion
- **Health Endpoints**: Accessible without authentication for monitoring purposes

## Development

### Running the API

```bash
dotnet restore
dotnet run
```

### Testing with Swagger

1. Navigate to `https://localhost:7001/swagger`
2. Click **Authorize** button
3. Enter your Bearer token: `Bearer <your_access_token>`
4. Test API endpoints

### Testing with HTTP Client

```http
GET https://localhost:7001/api/users/john.doe
Authorization: Bearer <your_access_token>
```

## Security Considerations

- **Client secrets** should never be committed to source control
- **Use HTTPS** in production environments
- **Implement proper token validation** and expiration handling
- **Consider implementing rate limiting** for API endpoints
- **Audit logging** for sensitive operations

## Troubleshooting

### Common Issues

1. **Authentication failed**: Check tenant ID, client ID, and client secret
2. **Insufficient permissions**: Ensure proper API permissions are granted
3. **Token expired**: Implement token refresh logic in your client application
4. **CORS issues**: Verify CORS configuration matches your client origin

### Debug Information

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Identity.Web": "Debug"
    }
  }
}
```

## Support

For issues related to:
- **Microsoft Entra ID**: Check [Microsoft Entra ID documentation](https://docs.microsoft.com/en-us/azure/active-directory/)
- **API functionality**: Review the code and check logs
- **Authentication**: Verify configuration and permissions
