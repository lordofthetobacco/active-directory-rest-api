# Test script for database logging functionality
# This script tests the PostgreSQL database logging service

Write-Host "🧪 Testing Database Logging Service..." -ForegroundColor Green

# Test database connectivity
Write-Host "`n📊 Testing database connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/health/database" -Method GET -ErrorAction Stop
    Write-Host "✅ Database connectivity test passed" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Database connectivity test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test audit logging endpoints (if available)
Write-Host "`n📝 Testing audit logging endpoints..." -ForegroundColor Yellow

# Test getting logs by correlation ID
Write-Host "Testing: Get logs by correlation ID..." -ForegroundColor Gray
try {
    $testCorrelationId = "test-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/logs/correlation/$testCorrelationId" -Method GET -ErrorAction Stop
    Write-Host "✅ Get logs by correlation ID test passed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Get logs by correlation ID test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting logs by time range
Write-Host "Testing: Get logs by time range..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/logs/timerange?startTime=$startTime&endTime=$endTime" -Method GET -ErrorAction Stop
    Write-Host "✅ Get logs by time range test passed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Get logs by time range test: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n🎯 Database logging tests completed!" -ForegroundColor Green
Write-Host "Note: Some tests may fail if the API is not running or if the endpoints are not implemented yet." -ForegroundColor Gray
