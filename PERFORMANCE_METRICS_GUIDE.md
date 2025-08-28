# Performance Metrics System Guide

This guide explains the comprehensive performance metrics system that automatically tracks and analyzes API performance in real-time.

## 🎯 Overview

The performance metrics system automatically captures detailed performance data for every API request and stores it in a dedicated PostgreSQL table. This provides real-time insights into API performance, response times, error rates, and usage patterns.

## 🏗️ Architecture

### **Automatic Data Collection**
- **Zero Configuration**: Metrics are automatically collected for all API endpoints
- **Real-time Processing**: Performance data is captured and stored asynchronously
- **Dual Storage**: Metrics are stored in both the database and used for real-time analysis

### **Performance Categorization**
The system automatically categorizes performance into four tiers:
- **FAST**: < 100ms response time
- **NORMAL**: 100-500ms response time  
- **SLOW**: 500-2000ms response time
- **VERY_SLOW**: > 2000ms response time

## 📊 Metrics Collected

### **Request Metrics**
- Endpoint path and HTTP method
- Request timestamp (UTC)
- Request size in bytes
- User context and correlation ID
- Client IP address and user agent

### **Response Metrics**
- Response time in milliseconds
- HTTP status code
- Response size in bytes
- Success/failure status
- Error messages (if applicable)

### **Performance Analytics**
- P95 and P99 response times
- Average, minimum, and maximum response times
- Success rates by endpoint
- Performance distribution across categories
- Error rate analysis

## 🚀 API Endpoints

### **Core Metrics Endpoints**

#### **Get Latest Metrics**
```http
GET /api/performancemetrics/latest?limit=100
```
Returns the most recent performance metrics with optional limit.

#### **Get Metrics by Endpoint**
```http
GET /api/performancemetrics/endpoint/{endpoint}?limit=100
```
Returns performance metrics for a specific endpoint.

#### **Get Metrics by Time Range**
```http
GET /api/performancemetrics/timerange?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z&limit=100
```
Returns metrics within a specified time range.

#### **Get Metrics by Action**
```http
GET /api/performancemetrics/action/{action}?limit=100
```
Returns metrics for a specific action (e.g., "GetUser", "CreateGroup").

#### **Get Metrics by Performance Category**
```http
GET /api/performancemetrics/category/{category}?limit=100
```
Returns metrics for a specific performance category (FAST, NORMAL, SLOW, VERY_SLOW).

### **Analytics Endpoints**

#### **Endpoint Performance Summary**
```http
GET /api/performancemetrics/summary/endpoint/{endpoint}?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z
```
Provides comprehensive performance analysis for a specific endpoint including:
- Total request count
- Success/failure rates
- Response time statistics (avg, min, max, P95, P99)
- Performance category breakdown
- Status code distribution

#### **Overall Performance Summary**
```http
GET /api/performancemetrics/summary/overall?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z
```
Provides system-wide performance overview including:
- Total requests across all endpoints
- Overall success rates
- System-wide response time statistics
- Endpoint-by-endpoint breakdown

#### **Slowest Endpoints**
```http
GET /api/performancemetrics/slowest?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z&limit=10
```
Identifies the slowest performing endpoints with detailed metrics.

#### **Error Rate Analysis**
```http
GET /api/performancemetrics/errors?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z
```
Analyzes error rates across all endpoints with status code breakdowns.

#### **Metrics Count**
```http
GET /api/performancemetrics/count?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z
```
Returns the total count of performance metrics in the system.

## 📈 Response Format

All endpoints return a consistent JSON response structure:

```json
{
  "success": true,
  "data": {
    // Endpoint-specific data
  },
  "metadata": {
    "count": 100,
    "limit": 100,
    "timestamp": "2024-01-01T12:00:00Z",
    "timeRange": {
      "startTime": "2024-01-01T00:00:00Z",
      "endTime": "2024-01-02T00:00:00Z"
    }
  }
}
```

## 🔍 Example Usage Scenarios

### **Monitoring API Health**
```bash
# Get current performance overview
curl "http://localhost:5000/api/performancemetrics/summary/overall?startTime=$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)&endTime=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

### **Identifying Performance Issues**
```bash
# Find slowest endpoints in the last 24 hours
curl "http://localhost:5000/api/performancemetrics/slowest?startTime=$(date -u -d '1 day ago' +%Y-%m-%dT%H:%M:%SZ)&endTime=$(date -u +%Y-%m-%dT%H:%M:%SZ)&limit=5"
```

### **Endpoint-Specific Analysis**
```bash
# Analyze specific endpoint performance
curl "http://localhost:5000/api/performancemetrics/summary/endpoint/api/users?startTime=$(date -u -d '1 day ago' +%Y-%m-%dT%H:%M:%SZ)&endTime=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

### **Error Rate Monitoring**
```bash
# Monitor error rates across all endpoints
curl "http://localhost:5000/api/performancemetrics/errors?startTime=$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)&endTime=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

## 🛠️ Testing

### **Test Scripts**
Use the provided PowerShell test script to verify functionality:
```powershell
./test-performance-metrics.ps1
```

### **Manual Testing**
1. Start the application: `dotnet run`
2. Make API calls to generate performance data
3. Query the metrics endpoints to verify data collection
4. Test various time ranges and filters

## 📊 Database Schema

### **performance_metrics Table**
```sql
CREATE TABLE performance_metrics (
    id BIGINT PRIMARY KEY,
    endpoint VARCHAR(200) NOT NULL,
    http_method VARCHAR(10) NOT NULL,
    action VARCHAR(100) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    response_time_ms DOUBLE PRECISION NOT NULL,
    status_code INTEGER NOT NULL,
    request_size_bytes BIGINT,
    response_size_bytes BIGINT,
    correlation_id VARCHAR(50),
    user_context VARCHAR(500),
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    is_success BOOLEAN NOT NULL,
    error_message VARCHAR(1000),
    performance_category VARCHAR(20) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL
);
```

### **Optimized Indexes**
- Primary performance indexes on `timestamp`, `endpoint`, `action`
- Composite indexes for common query patterns
- Performance category and status code indexes
- Correlation ID tracking indexes

## 🔒 Security & Access Control

### **Authentication Required**
All performance metrics endpoints require JWT authentication with read permissions.

### **Data Privacy**
- Client IP addresses are captured for security analysis
- User context is limited to application identifiers
- No sensitive request/response data is stored
- Correlation IDs link related operations without exposing details

## 📈 Performance Considerations

### **Asynchronous Processing**
- Metrics collection is non-blocking
- Database writes are performed asynchronously
- No impact on API response times

### **Storage Optimization**
- Automatic cleanup of old metrics
- Efficient indexing for fast queries
- JSONB storage for flexible data structures

### **Scalability**
- Designed for high-volume API operations
- Efficient aggregation queries
- Partitioning-ready schema design

## 🚨 Monitoring & Alerts

### **Key Performance Indicators**
- **Response Time Thresholds**: Monitor for SLOW and VERY_SLOW categories
- **Error Rate Thresholds**: Alert on error rates above acceptable levels
- **Endpoint Performance**: Track performance degradation over time

### **Alerting Recommendations**
- P95 response time > 1000ms
- Error rate > 5% for any endpoint
- Performance category shift from FAST to SLOW
- Unusual request volume spikes

## 🔄 Maintenance

### **Data Retention**
- Configure automatic cleanup of old metrics
- Recommended retention: 90 days for detailed metrics, 1 year for aggregated data

### **Performance Optimization**
- Regular table optimization and index maintenance
- Monitor query performance and adjust indexes as needed

## 📚 Integration Examples

### **Grafana Dashboard**
```sql
-- Example query for Grafana time series
SELECT 
    time_bucket('5 minutes', timestamp) AS time,
    endpoint,
    AVG(response_time_ms) as avg_response_time,
    COUNT(*) as request_count
FROM performance_metrics 
WHERE timestamp > $__timeFrom() AND timestamp < $__timeTo()
GROUP BY time, endpoint
ORDER BY time;
```

### **Prometheus Metrics**
The system can be extended to export Prometheus metrics for integration with monitoring stacks.

## 🎯 Best Practices

1. **Regular Monitoring**: Check performance metrics daily
2. **Trend Analysis**: Monitor performance changes over time
3. **Capacity Planning**: Use metrics to plan infrastructure scaling
4. **Incident Response**: Use correlation IDs to trace performance issues
5. **Performance Budgets**: Set and monitor performance targets

---

**Note**: This performance metrics system provides comprehensive visibility into API performance without any additional instrumentation required. All data is automatically collected and available for real-time analysis and historical trending.
