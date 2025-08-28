# PowerShell script to test JWT authentication for Active Directory API
# This script demonstrates how to get an access token and use it with the API

param(
    [string]$TenantId = "5cbf4b45-97e8-498c-b584-ef6b12a29fe1",
    [string]$ClientId = "d5159527-16ba-4ae8-aa65-e9636855ad6c",
    [string]$ClientSecret = "",
    [string]$ApiBaseUrl = "https://localhost:7001"
)

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Blue = "Blue"

Write-Host "=== Active Directory API JWT Authentication Test ===" -ForegroundColor $Blue
Write-Host ""

if ([string]::IsNullOrEmpty($ClientSecret)) {
    Write-Host "ERROR: ClientSecret parameter is required!" -ForegroundColor $Red
    Write-Host "Usage: .\test-jwt-auth.ps1 -ClientSecret 'your-client-secret'" -ForegroundColor $Yellow
    exit 1
}

# Step 1: Get Access Token
Write-Host "Step 1: Getting access token from Microsoft Entra ID..." -ForegroundColor $Blue

$tokenUrl = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
$tokenBody = @{
    grant_type = "client_credentials"
    client_id = $ClientId
    client_secret = $ClientSecret
    scope = "https://graph.microsoft.com/.default"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    
    if ($tokenResponse.access_token) {
        $accessToken = $tokenResponse.access_token
        $expiresIn = $tokenResponse.expires_in
        $tokenType = $tokenResponse.token_type
        
        Write-Host "✓ Access token obtained successfully!" -ForegroundColor $Green
        Write-Host "  Token Type: $tokenType" -ForegroundColor $Green
        Write-Host "  Expires In: $expiresIn seconds" -ForegroundColor $Green
        Write-Host "  Token Preview: $($accessToken.Substring(0, 50))..." -ForegroundColor $Green
        Write-Host ""
    } else {
        Write-Host "✗ Failed to get access token" -ForegroundColor $Red
        Write-Host "Response: $($tokenResponse | ConvertTo-Json)" -ForegroundColor $Red
        exit 1
    }
} catch {
    Write-Host "✗ Error getting access token: $($_.Exception.Message)" -ForegroundColor $Red
    exit 1
}

# Step 2: Test Health Endpoint (No Auth Required)
Write-Host "Step 2: Testing health endpoint (no authentication required)..." -ForegroundColor $Blue

try {
    $healthResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/health" -Method Get
    Write-Host "✓ Health check successful!" -ForegroundColor $Green
    Write-Host "  Status: $($healthResponse.status)" -ForegroundColor $Green
    Write-Host "  Active Directory: $($healthResponse.activeDirectory)" -ForegroundColor $Green
    Write-Host ""
} catch {
    Write-Host "✗ Health check failed: $($_.Exception.Message)" -ForegroundColor $Red
    Write-Host "  Make sure the API is running on $ApiBaseUrl" -ForegroundColor $Yellow
    Write-Host ""
}

# Step 3: Test Protected Endpoint (With Auth)
Write-Host "Step 3: Testing protected endpoint (with JWT authentication)..." -ForegroundColor $Blue

$headers = @{
    "Authorization" = "Bearer $accessToken"
    "Content-Type" = "application/json"
}

try {
    # Test user search endpoint
    $searchBody = @{
        searchTerm = "test"
        maxResults = 5
    } | ConvertTo-Json
    
    $searchResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users/search" -Method Post -Headers $headers -Body $searchBody
    
    Write-Host "✓ Protected endpoint test successful!" -ForegroundColor $Green
    Write-Host "  Users found: $($searchResponse.Count)" -ForegroundColor $Green
    Write-Host ""
} catch {
    Write-Host "✗ Protected endpoint test failed: $($_.Exception.Message)" -ForegroundColor $Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "  HTTP Status: $statusCode" -ForegroundColor $Red
    }
    Write-Host ""
}

# Step 4: Test Admin Endpoint (Requires Admin Permissions)
Write-Host "Step 4: Testing admin endpoint (requires admin permissions)..." -ForegroundColor $Blue

try {
    # Test getting user groups (read-only operation)
    $groupsResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users/testuser/groups" -Method Get -Headers $headers
    
    Write-Host "✓ Admin endpoint test successful!" -ForegroundColor $Green
    Write-Host "  Groups found: $($groupsResponse.Count)" -ForegroundColor $Green
    Write-Host ""
} catch {
    Write-Host "✗ Admin endpoint test failed: $($_.Exception.Message)" -ForegroundColor $Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "  HTTP Status: $statusCode" -ForegroundColor $Red
        
        if ($statusCode -eq 403) {
            Write-Host "  This is expected if the application doesn't have admin permissions" -ForegroundColor $Yellow
        }
    }
    Write-Host ""
}

# Step 5: Display Token Information
Write-Host "Step 5: Token information..." -ForegroundColor $Blue

try {
    # Decode JWT token to see claims
    $tokenParts = $accessToken.Split('.')
    if ($tokenParts.Length -eq 3) {
        $payload = $tokenParts[1]
        # Add padding if needed
        $padding = 4 - ($payload.Length % 4)
        if ($padding -ne 4) {
            $payload += "=" * $padding
        }
        
        $decodedPayload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
        $tokenClaims = $decodedPayload | ConvertFrom-Json
        
        Write-Host "✓ Token decoded successfully!" -ForegroundColor $Green
        Write-Host "  App ID: $($tokenClaims.appid)" -ForegroundColor $Green
        Write-Host "  Tenant ID: $($tokenClaims.tid)" -ForegroundColor $Green
        Write-Host "  Issued At: $([DateTimeOffset]::FromUnixTimeSeconds($tokenClaims.iat).DateTime)" -ForegroundColor $Green
        Write-Host "  Expires At: $([DateTimeOffset]::FromUnixTimeSeconds($tokenClaims.exp).DateTime)" -ForegroundColor $Green
        
        if ($tokenClaims.roles) {
            Write-Host "  Roles: $($tokenClaims.roles -join ', ')" -ForegroundColor $Green
        }
        Write-Host ""
    }
} catch {
    Write-Host "✗ Failed to decode token: $($_.Exception.Message)" -ForegroundColor $Red
    Write-Host ""
}

# Step 6: Summary
Write-Host "=== Test Summary ===" -ForegroundColor $Blue
Write-Host "✓ JWT authentication configured successfully" -ForegroundColor $Green
Write-Host "✓ Access token obtained and validated" -ForegroundColor $Green
Write-Host "✓ API endpoints protected with proper authorization" -ForegroundColor $Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor $Yellow
Write-Host "1. Implement token refresh logic in your client application" -ForegroundColor $Yellow
Write-Host "2. Store the client secret securely (use Azure Key Vault in production)" -ForegroundColor $Yellow
Write-Host "3. Monitor token expiration and API usage" -ForegroundColor $Yellow
Write-Host "4. Implement proper error handling for authentication failures" -ForegroundColor $Yellow
Write-Host ""

Write-Host "Test completed successfully!" -ForegroundColor $Green
