# Test script for Performance Metrics endpoints
# This script tests the new performance metrics functionality

Write-Host "🧪 Testing Performance Metrics Endpoints..." -ForegroundColor Green

# Test database connectivity for performance metrics
Write-Host "`n📊 Testing performance metrics database connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/count" -Method GET -ErrorAction Stop
    Write-Host "✅ Performance metrics database connectivity test passed" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Performance metrics database connectivity test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test getting latest metrics
Write-Host "`n📝 Testing: Get latest performance metrics..." -ForegroundColor Gray
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/latest?limit=10" -Method GET -ErrorAction Stop
    Write-Host "✅ Get latest metrics test passed" -ForegroundColor Green
    Write-Host "Metrics count: $($response.data.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get latest metrics test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting metrics by endpoint
Write-Host "Testing: Get metrics by endpoint..." -ForegroundColor Gray
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/endpoint/api/health?limit=5" -Method GET -ErrorAction Stop
    Write-Host "✅ Get metrics by endpoint test passed" -ForegroundColor Green
    Write-Host "Metrics count: $($response.data.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get metrics by endpoint test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting metrics by time range
Write-Host "Testing: Get metrics by time range..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/timerange?startTime=$startTime&endTime=$endTime&limit=5" -Method GET -ErrorAction Stop
    Write-Host "✅ Get metrics by time range test passed" -ForegroundColor Green
    Write-Host "Metrics count: $($response.data.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get metrics by time range test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting performance summary
Write-Host "Testing: Get endpoint performance summary..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/summary/endpoint/api/health?startTime=$startTime&endTime=$endTime" -Method GET -ErrorAction Stop
    Write-Host "✅ Get endpoint summary test passed" -ForegroundColor Green
    Write-Host "Summary data: $($response.data | ConvertTo-Json -Depth 2)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get endpoint summary test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting overall performance summary
Write-Host "Testing: Get overall performance summary..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/summary/overall?startTime=$startTime&endTime=$endTime" -Method GET -ErrorAction Stop
    Write-Host "✅ Get overall summary test passed" -ForegroundColor Green
    Write-Host "Overall summary: $($response.data | ConvertTo-Json -Depth 2)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get overall summary test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting slowest endpoints
Write-Host "Testing: Get slowest endpoints..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/slowest?startTime=$startTime&endTime=$endTime&limit=5" -Method GET -ErrorAction Stop
    Write-Host "✅ Get slowest endpoints test passed" -ForegroundColor Green
    Write-Host "Slowest endpoints: $($response.data.slowestEndpoints | ConvertTo-Json -Depth 2)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get slowest endpoints test: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Test getting error rates
Write-Host "Testing: Get error rates..." -ForegroundColor Gray
try {
    $startTime = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/performancemetrics/errors?startTime=$startTime&endTime=$endTime" -Method GET -ErrorAction Stop
    Write-Host "✅ Get error rates test passed" -ForegroundColor Green
    Write-Host "Error rates: $($response.data.errorRates | ConvertTo-Json -Depth 2)" -ForegroundColor Cyan
} catch {
    Write-Host "⚠️  Get error rates test: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n🎯 Performance metrics tests completed!" -ForegroundColor Green
Write-Host "Note: Some tests may fail if the API is not running or if there's no performance data yet." -ForegroundColor Gray
Write-Host "Make some API calls first to generate performance metrics data." -ForegroundColor Gray
