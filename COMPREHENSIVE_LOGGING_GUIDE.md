# Comprehensive Logging System Guide

## 🎯 **Overview**

This guide documents the comprehensive logging system implemented for the Active Directory REST API. The system addresses all previously identified gaps and provides robust audit trails for every API operation.

## ✅ **Gaps Addressed**

| Gap | Status | Solution |
|-----|--------|----------|
| ❌ No request/response logging | ✅ **FIXED** | Comprehensive request/response logging with correlation IDs |
| ❌ No user/application identification | ✅ **FIXED** | JWT claims extraction and user context logging |
| ❌ No performance metrics | ✅ **FIXED** | Stopwatch-based timing for all operations |
| ❌ No audit trail for admin operations | ✅ **FIXED** | Authorization success/failure logging |
| ❌ Inconsistent error logging | ✅ **FIXED** | Standardized error logging patterns |

## 🏗️ **Architecture**

### **Core Components**

1. **`IAuditLoggingService`** - Interface defining logging operations
2. **`AuditLoggingService`** - Implementation with structured logging
3. **`BaseController`** - Base class providing logging infrastructure
4. **Correlation ID Middleware** - Request tracking across operations

### **Logging Flow**

```
Request → Correlation ID Generation → Request Logging → Operation Execution → Response Logging → Correlation ID Return
```

## 📊 **What Gets Logged**

### **1. Request Information**
- **Action**: What operation is being performed (e.g., "GetUser", "CreateUser")
- **Resource**: Target resource identifier (e.g., "User:john.doe", "Group:Administrators")
- **Request Data**: Input parameters (sensitive data masked)
- **User Context**: Authenticated user/application information
- **Correlation ID**: Unique identifier for request tracking

### **2. Response Information**
- **Status Code**: HTTP response status (200, 404, 400, 500, etc.)
- **Response Data**: Returned data or error information
- **Duration**: Operation execution time in milliseconds
- **User Context**: Who performed the operation
- **Correlation ID**: Links to original request

### **3. Error Information**
- **Exception Details**: Full exception information with stack traces
- **Request Context**: What was being attempted when error occurred
- **User Context**: Who encountered the error
- **Correlation ID**: Links error to specific request

### **4. Authorization Events**
- **Success**: When admin operations are authorized
- **Failure**: When operations are denied with reasons
- **User Context**: Who attempted the operation
- **Resource**: What resource was being accessed

### **5. Active Directory Operations**
- **Operation Type**: What AD operation was performed
- **Target**: Which user/group was affected
- **Success/Failure**: Operation outcome
- **Duration**: How long the operation took
- **Error Details**: Specific error messages for failures

## 🔧 **Implementation Details**

### **Base Controller Methods**

```csharp
// Log request details
protected void LogRequest(string action, string resource, object? requestData)

// Log response details
protected void LogResponse(string action, string resource, object? responseData, int statusCode, TimeSpan duration)

// Log error details
protected void LogError(string action, string resource, Exception exception, object? requestData)

// Log authorization success
protected void LogAuthorizationSuccess(string action, string resource)

// Log authorization failure
protected void LogAuthorizationFailure(string action, string resource, string reason)

// Log Active Directory operations
protected void LogActiveDirectoryOperation(string operation, string target, bool success, TimeSpan duration, string? errorMessage = null)
```

### **Controller Implementation Pattern**

```csharp
[HttpGet("{samAccountName}")]
public async Task<ActionResult<ActiveDirectoryUser>> GetUser(string samAccountName)
{
    var stopwatch = Stopwatch.StartNew();
    var action = "GetUser";
    var resource = $"User:{samAccountName}";
    var requestData = new { samAccountName };

    try
    {
        // Log the request
        LogRequest(action, resource, requestData);

        // Execute the operation
        var user = await _adService.GetUserAsync(samAccountName);
        stopwatch.Stop();

        if (user == null)
        {
            LogResponse(action, resource, null, 404, stopwatch.Elapsed);
            return NotFound();
        }

        // Log successful response
        LogResponse(action, resource, user, 200, stopwatch.Elapsed);
        return Ok(user);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        LogError(action, resource, ex, requestData);
        throw;
    }
}
```

## 🎭 **User Context Extraction**

### **Machine-to-Machine Authentication**
- **App ID**: Extracted from JWT `appid` claim
- **Format**: `app:{app-id}`

### **User Authentication**
- **Username**: Extracted from JWT identity
- **Roles**: Extracted from JWT `roles` claim
- **Format**: `user:{username} (roles: role1, role2)`

### **Unauthenticated Requests**
- **Format**: `unauthenticated`

## 🔒 **Security Features**

### **Sensitive Data Masking**
- **Passwords**: Always masked as `***MASKED***`
- **Client Secrets**: Masked in request logs
- **Tokens**: Masked in request logs
- **Other Secrets**: Automatically detected and masked

### **Data Truncation**
- **Request Data**: Truncated at 1000 characters
- **Response Data**: Truncated at 1000 characters
- **Error Messages**: Full exception details preserved

## 📈 **Performance Monitoring**

### **Timing Metrics**
- **Request Duration**: Total time from request to response
- **AD Operation Duration**: Time spent in Active Directory operations
- **Granularity**: Millisecond precision

### **Performance Insights**
- **Slow Operations**: Identify operations taking >1000ms
- **AD Performance**: Monitor Active Directory response times
- **API Performance**: Track overall API responsiveness

## 🔍 **Correlation ID System**

### **How It Works**
1. **Generation**: Unique ID generated for each request
2. **Propagation**: ID passed through all logging operations
3. **Response Header**: ID returned in `X-Correlation-ID` header
4. **Client Usage**: Clients can include ID in subsequent requests

### **Benefits**
- **Request Tracking**: Follow single request through entire system
- **Debugging**: Correlate logs across multiple operations
- **Monitoring**: Track request flow and identify bottlenecks

## 📝 **Log Format Examples**

### **Request Log**
```
API Request: GetUser on User:john.doe | User: app:d5159527-16ba-4ae8-aa65-e9636855ad6c | CorrelationId: a1b2c3d4e5f6 | Request: {"samAccountName":"john.doe"}
```

### **Response Log**
```
API Response: GetUser on User:john.doe | Status: 200 | Duration: 45ms | User: app:d5159527-16ba-4ae8-aa65-e9636855ad6c | CorrelationId: a1b2c3d4e5f6 | Response: {"samAccountName":"john.doe","displayName":"John Doe",...}
```

### **Error Log**
```
API Error: GetUser on User:john.doe | User: app:d5159527-16ba-4ae8-aa65-e9636855ad6c | CorrelationId: a1b2c3d4e5f6 | Request: {"samAccountName":"john.doe"} | Error: User not found in Active Directory
```

### **Authorization Log**
```
Authorization Success: CreateUser on User:newuser | User: app:d5159527-16ba-4ae8-aa65-e9636855ad6c
```

### **Active Directory Operation Log**
```
Active Directory Operation: GetUser on john.doe | Success | Duration: 23ms | CorrelationId: a1b2c3d4e5f6
```

## 🚀 **Getting Started**

### **1. Enable Logging in Configuration**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ActiveDirectory_API.Services.AuditLoggingService": "Information",
      "ActiveDirectory_API.Services.ActiveDirectoryService": "Information"
    }
  }
}
```

### **2. Use in Controllers**

```csharp
public class MyController : BaseController
{
    public MyController(IAuditLoggingService auditLogger, ILogger<MyController> logger) 
        : base(auditLogger, logger)
    {
    }

    [HttpGet]
    public async Task<ActionResult<MyData>> GetData()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetData";
        var resource = "MyResource";
        var requestData = new { /* request parameters */ };

        try
        {
            LogRequest(action, resource, requestData);
            
            // Your operation here
            var result = await _service.GetDataAsync();
            stopwatch.Stop();
            
            LogResponse(action, resource, result, 200, stopwatch.Elapsed);
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }
}
```

### **3. Monitor Logs**

```bash
# View all API requests
grep "API Request" logs/app.log

# View errors for specific user
grep "User:john.doe" logs/app.log | grep "API Error"

# View performance metrics
grep "Duration:" logs/app.log | awk '{print $NF}' | sort -n

# Track specific request
grep "a1b2c3d4e5f6" logs/app.log
```

## 🔧 **Configuration Options**

### **Logging Levels**
- **Information**: All API operations, responses, and performance data
- **Warning**: Authorization failures and non-critical errors
- **Error**: Exceptions and critical failures
- **Debug**: Detailed operation tracing (development only)

### **Performance Thresholds**
- **Slow Request Warning**: Operations > 1000ms
- **AD Operation Warning**: AD operations > 500ms
- **Response Time Tracking**: All operations timed

## 📊 **Monitoring and Alerting**

### **Key Metrics to Monitor**
1. **Request Volume**: Total API requests per minute
2. **Error Rate**: Percentage of failed requests
3. **Response Times**: Average and 95th percentile response times
4. **Authorization Failures**: Rate of denied operations
5. **AD Performance**: Active Directory operation success rates

### **Alerting Rules**
- **High Error Rate**: >5% error rate for 5 minutes
- **Slow Response**: >2000ms average response time
- **AD Failures**: >10% AD operation failure rate
- **Auth Failures**: >20 authorization failures per minute

## 🧪 **Testing the Logging System**

### **1. Test Request Logging**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
     -H "X-Correlation-ID: test-123" \
     http://localhost:5000/api/users/john.doe
```

### **2. Test Error Logging**
```bash
curl -H "Authorization: Bearer INVALID_TOKEN" \
     http://localhost:5000/api/users/nonexistent
```

### **3. Test Performance Logging**
```bash
# This will show timing information in logs
curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:5000/api/users/search \
     -d '{"searchTerm":"test","maxResults":100}'
```

## 🔮 **Future Enhancements**

### **Planned Features**
1. **Log Aggregation**: Centralized log collection and analysis
2. **Real-time Monitoring**: Live dashboard for API performance
3. **Advanced Analytics**: Machine learning for anomaly detection
4. **Compliance Reporting**: Automated audit trail generation
5. **Integration**: SIEM system integration for security monitoring

### **Customization Options**
1. **Custom Log Formats**: Configurable log message templates
2. **Selective Logging**: Enable/disable specific log types
3. **External Logging**: Send logs to external systems
4. **Log Retention**: Configurable log storage policies

## 📚 **Best Practices**

### **1. Consistent Naming**
- Use descriptive action names (e.g., "GetUser" not "Get")
- Use hierarchical resource naming (e.g., "User:john.doe:Groups")
- Maintain consistent casing and formatting

### **2. Sensitive Data Handling**
- Always mask passwords and secrets
- Be careful with user input in logs
- Consider PII implications of logged data

### **3. Performance Considerations**
- Keep log messages concise but informative
- Avoid expensive operations in logging code
- Use structured logging for better performance

### **4. Monitoring and Maintenance**
- Regularly review log volumes and performance
- Set up log rotation and retention policies
- Monitor disk space usage
- Set up alerts for logging system failures

## 🆘 **Troubleshooting**

### **Common Issues**

1. **Missing Logs**
   - Check logging configuration
   - Verify service registration
   - Check log file permissions

2. **Performance Impact**
   - Review log message complexity
   - Check logging level settings
   - Monitor disk I/O performance

3. **Correlation ID Issues**
   - Verify middleware registration order
   - Check header propagation
   - Validate ID generation logic

### **Debug Commands**

```bash
# Check if logging service is registered
dotnet run -- --help | grep logging

# Verify log file creation
ls -la logs/

# Test correlation ID middleware
curl -v -H "X-Correlation-ID: test" http://localhost:5000/api/health

# Monitor real-time logs
tail -f logs/app.log | grep "API Request"
```

---

## 📞 **Support**

For questions or issues with the logging system:

1. **Check Logs**: Review application logs for error details
2. **Verify Configuration**: Ensure logging settings are correct
3. **Test Endpoints**: Use provided test commands to verify functionality
4. **Review Documentation**: Check this guide for implementation details

The comprehensive logging system provides complete visibility into all API operations, enabling effective monitoring, debugging, and compliance reporting.
