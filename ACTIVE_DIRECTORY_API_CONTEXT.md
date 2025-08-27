# Active Directory REST API - Complete Context & Architecture

## 🏗️ **APPLICATION OVERVIEW**

This is a **Windows-only** .NET 8.0 REST API that provides secure access to Active Directory (AD) operations through Microsoft Entra ID (Azure AD) authentication. The API acts as a secure proxy between clients and on-premises Active Directory, enabling cloud-based applications to manage AD users and groups.

## 🎯 **CORE PURPOSE**

- **Secure AD Management**: Provide REST API access to Active Directory operations
- **Cloud Authentication**: Use Microsoft Entra ID for secure API access
- **Role-Based Access Control**: Different permission levels for users vs administrators
- **Windows Integration**: Leverage native Windows AD libraries and services

## 🏛️ **ARCHITECTURE PATTERNS**

### **Layered Architecture**
```
Controllers (API Layer)
    ↓
Services (Business Logic)
    ↓
System.DirectoryServices (Windows AD Integration)
```

### **Dependency Injection Pattern**
- Services registered as scoped/singleton in `Program.cs`
- Configuration injected via `ActiveDirectoryConfiguration`
- Platform-specific service registration (Windows-only)

### **Authentication & Authorization Flow**
```
Client Request → JWT Token Validation → Claims Extraction → Policy Enforcement → AD Operation
```

## 🔧 **TECHNICAL STACK**

### **Framework & Runtime**
- **.NET 8.0** with ASP.NET Core
- **Windows Runtime** (`RuntimeIdentifier: win-x64`)
- **Nullable Reference Types** enabled
- **Implicit Usings** enabled

### **Key Dependencies**
```xml
- Microsoft.AspNetCore.OpenApi (8.0.19)
- Swashbuckle.AspNetCore (6.6.2) - Swagger/OpenAPI
- System.DirectoryServices.Protocols (8.0.0) - LDAP operations
- System.DirectoryServices.AccountManagement (8.0.0) - AD user/group management
- Microsoft.Identity.Web (2.17.1) - Entra ID integration
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0) - JWT validation
```

### **Platform Constraints**
- **Windows-only** due to AD service dependencies
- **Runtime validation** prevents startup on non-Windows platforms
- **Platform-specific attributes** (`[SupportedOSPlatform("windows")]`)

## 🗂️ **PROJECT STRUCTURE**

### **Root Level**
```
ActiveDirectory_API/
├── Program.cs (Main entry point & configuration)
├── ActiveDirectory_API.csproj (Project file)
├── appsettings.Development.json (Development config)
├── ActiveDirectory_API.http (HTTP client examples)
├── README.md (Comprehensive documentation)
└── tests/ (Test configuration)
```

### **Controllers Layer** (`/Controllers/`)
- **`AuthController.cs`** - Authentication & user info endpoints
- **`UsersController.cs`** - User management operations
- **`GroupsController.cs`** - Group management operations  
- **`HealthController.cs`** - Health monitoring endpoints

### **Models Layer** (`/Models/`)
- **`ActiveDirectoryConfiguration.cs`** - AD connection settings
- **`ActiveDirectoryUser.cs`** - User entity model
- **`ActiveDirectoryGroup.cs`** - Group entity model
- **`ActiveDirectorySearchRequest.cs`** - Search parameters
- **`EntraIdConfiguration.cs`** - Microsoft Entra ID settings

### **Services Layer** (`/Services/`)
- **`IActiveDirectoryService.cs`** - Service contract interface
- **`ActiveDirectoryService.cs`** - Windows AD implementation

## 🔐 **AUTHENTICATION & SECURITY**

### **Microsoft Entra ID Integration**
- **JWT Bearer Token** authentication
- **Token acquisition** for downstream API calls
- **Microsoft Graph API** integration
- **Role-based claims** extraction

### **Authorization Policies**
```csharp
- "RequireAuthenticatedUser" - Basic authentication required
- "RequireAdminRole" - Admin/Global Administrator role required
```

### **Security Headers**
- **CORS** configured for development (`AllowAll` policy)
- **HTTPS redirection** enabled
- **JWT validation** with proper error handling

## 📊 **DATA MODELS**

### **ActiveDirectoryUser**
```csharp
- DistinguishedName, SamAccountName, DisplayName
- GivenName, Surname, Email, UserPrincipalName
- Enabled, LastLogon, PasswordLastSet, AccountExpires
- MemberOf[], Department, Title, Office, Phone, Mobile, Manager
```

### **ActiveDirectoryGroup**
```csharp
- DistinguishedName, Name, SamAccountName, Description
- GroupType, Scope, Members[], MemberOf[]
- Manager, WhenCreated, WhenChanged
```

### **ActiveDirectorySearchRequest**
```csharp
- SearchTerm, SearchBase, Filter, Attributes[]
- MaxResults, SortBy, SortAscending
```

## 🚀 **API ENDPOINTS**

### **Authentication** (`/api/auth/`)
- `GET /me` - Current user information
- `GET /roles` - User roles and permissions
- `POST /logout` - User logout

### **User Management** (`/api/users/`)
- `GET /{samAccountName}` - Get user by SAM account
- `GET /email/{email}` - Get user by email
- `GET /dn/{distinguishedName}` - Get user by DN
- `POST /search` - Search users with filters
- `POST /authenticate` - Validate user credentials
- `GET /{username}/groups` - Get user's group memberships

### **Group Management** (`/api/groups/`)
- `GET /{groupName}` - Get group information
- `POST /search` - Search groups with filters
- `GET /{groupName}/members/{username}/check` - Check membership
- `POST /{groupName}/members/{username}` - Add user to group (Admin)
- `DELETE /{groupName}/members/{username}` - Remove user from group (Admin)

### **Health Monitoring** (`/api/health/`)
- `GET /` - Overall API health status
- `GET /ad` - Active Directory connectivity status

## ⚙️ **CONFIGURATION MANAGEMENT**

### **Active Directory Settings**
```json
{
  "ActiveDirectory": {
    "Server": "ldap://your-domain-controller",
    "Port": 389,
    "UseSSL": false,
    "BindDN": "CN=ServiceAccount,OU=ServiceAccounts,DC=domain,DC=com",
    "BindPassword": "service-account-password",
    "SearchBase": "DC=domain,DC=com",
    "Timeout": 30,
    "UseIntegratedSecurity": false
  }
}
```

### **Microsoft Entra ID Settings**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

## 🔄 **SERVICE OPERATIONS**

### **User Operations**
- **CRUD Operations**: Create, Read, Update, Delete users
- **Authentication**: Validate user credentials
- **Group Management**: Add/remove users from groups
- **Account Control**: Enable/disable accounts, unlock users
- **Password Management**: Reset user passwords
- **Search & Filtering**: Advanced user search capabilities

### **Group Operations**
- **Group Information**: Retrieve group details and membership
- **Member Management**: Add/remove group members
- **Search & Discovery**: Find groups by name or criteria
- **Hierarchy Support**: Nested group relationships

### **Connection Management**
- **Startup Validation**: Verify AD connectivity before app start
- **Health Monitoring**: Continuous connection status checking
- **Error Handling**: Graceful degradation on connection failures

## 🧪 **TESTING & DEVELOPMENT**

### **Swagger Integration**
- **Interactive API documentation** at `/swagger`
- **JWT authentication support** in Swagger UI
- **Request/response examples** for all endpoints
- **Authorization header** configuration

### **HTTP Client Examples**
- **`ActiveDirectory_API.http`** file with sample requests
- **Authentication examples** with Bearer tokens
- **Endpoint testing** scenarios

### **Development Environment**
- **Development configuration** in `appsettings.Development.json`
- **CORS enabled** for local development
- **Detailed logging** for debugging
- **HTTPS with local certificates**

## 🚨 **ERROR HANDLING & LOGGING**

### **Exception Management**
- **Graceful degradation** on AD connection failures
- **Structured logging** with correlation IDs
- **User-friendly error messages** for API consumers
- **Critical error prevention** during startup

### **Health Check Integration**
- **Startup validation** prevents app start on AD failures
- **Continuous monitoring** of AD connectivity
- **Response time tracking** for performance insights
- **Error details** in health check responses

## 🔒 **SECURITY CONSIDERATIONS**

### **Production Hardening**
- **Environment variables** for sensitive configuration
- **Azure Key Vault** integration for secrets
- **HTTPS enforcement** in production
- **Rate limiting** implementation
- **Audit logging** for sensitive operations

### **Access Control**
- **JWT token validation** with proper expiration
- **Role-based permissions** for administrative operations
- **Principle of least privilege** enforcement
- **Secure configuration** management

## 📈 **PERFORMANCE & SCALABILITY**

### **Optimization Strategies**
- **Async/await patterns** throughout the codebase
- **Connection pooling** for AD operations
- **Efficient search algorithms** with result limiting
- **Caching considerations** for frequently accessed data

### **Monitoring & Observability**
- **Health check endpoints** for load balancer integration
- **Response time tracking** for performance monitoring
- **Structured logging** for log aggregation
- **Error rate monitoring** through health checks

## 🔄 **DEPLOYMENT & OPERATIONS**

### **Deployment Requirements**
- **Windows Server** environment
- **Active Directory connectivity** from deployment location
- **Microsoft Entra ID** application registration
- **SSL certificates** for HTTPS

### **Configuration Management**
- **Environment-specific** configuration files
- **Secret management** through secure channels
- **Health check integration** with monitoring systems
- **Log aggregation** setup

## 🎯 **USE CASES & SCENARIOS**

### **Enterprise Applications**
- **HR Systems**: User provisioning and management
- **Identity Management**: Centralized user administration
- **Access Control**: Group-based permission management
- **Audit & Compliance**: User activity tracking

### **Integration Scenarios**
- **Cloud Applications**: Secure AD access from cloud services
- **API Gateway**: Centralized AD management API
- **Automation**: Scripted user and group management
- **Monitoring**: AD health and performance monitoring

## 🔮 **FUTURE ENHANCEMENTS**

### **Potential Improvements**
- **Multi-forest support** for complex AD environments
- **Advanced search capabilities** with LDAP filters
- **Bulk operations** for large-scale changes
- **Webhook notifications** for AD changes
- **Audit trail** for all operations
- **Performance metrics** and analytics

### **Integration Opportunities**
- **PowerShell integration** for advanced scripting
- **GraphQL support** for flexible data queries
- **Event streaming** for real-time AD changes
- **Machine learning** for user behavior analysis

---

## 📝 **MEMORIZATION KEY POINTS**

1. **Windows-only** due to AD service dependencies
2. **JWT authentication** via Microsoft Entra ID
3. **Role-based access control** with Admin policies
4. **Startup validation** prevents app start on AD failures
5. **Comprehensive CRUD operations** for users and groups
6. **Health monitoring** with connection validation
7. **Swagger documentation** with authentication support
8. **Async patterns** throughout for performance
9. **Structured logging** for operational visibility
10. **CORS enabled** for development flexibility

This API serves as a **secure bridge** between cloud applications and on-premises Active Directory, enabling modern application architectures while maintaining enterprise security standards.
