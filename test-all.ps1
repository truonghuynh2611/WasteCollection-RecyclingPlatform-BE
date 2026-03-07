# ALL-IN-ONE TEST SCRIPT
# Waste Collection Platform - Comprehensive API Tests

param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  COMPREHENSIVE API TEST SUITE" -ForegroundColor Cyan
Write-Host "  Waste Collection Platform" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

$testsPassed = 0
$testsFailed = 0
$testResults = @()

# Helper function
function Test-Endpoint {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    Write-Host "================================================" -ForegroundColor Gray
    Write-Host "TEST: $Name" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Gray
    
    try {
        $result = & $Test
        Write-Host "✅ PASSED" -ForegroundColor Green
        Write-Host $result -ForegroundColor White
        $script:testsPassed++
        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = "PASSED"
            Details = $result
        }
    }
    catch {
        Write-Host "❌ FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        $script:testsFailed++
        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = "FAILED"
            Details = $_.Exception.Message
        }
    }
    
    Write-Host ""
}

# ============================================================
# TEST 1: GET /api/Team
# ============================================================
$teams = $null
$firstTeamId = $null

Test-Endpoint -Name "GET /api/Team - List all teams" -Test {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/Team" -Method GET
    
    if ($response.success -ne $true) {
        throw "API returned success: false"
    }
    
    $script:teams = $response.data
    $script:firstTeamId = $teams[0].teamId
    
    "   Team count: $($teams.Count)`n   First team ID: $firstTeamId`n   Sample teams:`n$(($teams[0..2] | ForEach-Object { "     • ID: $($_.teamId) | Name: $($_.name)" }) -join "`n")"
}

# ============================================================
# TEST 2: GET /api/Team/{id}
# ============================================================
Test-Endpoint -Name "GET /api/Team/{id} - Get specific team" -Test {
    if ($null -eq $firstTeamId) {
        throw "No team ID available from previous test"
    }
    
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/Team/$firstTeamId" -Method GET
    
    if ($response.success -ne $true) {
        throw "API returned success: false"
    }
    
    "   Team ID: $($response.data.teamId)`n   Name: $($response.data.name)`n   Area ID: $($response.data.areaId)"
}

# ============================================================
# TEST 3: POST /api/Auth/register - Citizen
# ============================================================
$citizenEmail = $null

Test-Endpoint -Name "POST /api/Auth/register - Register Citizen" -Test {
    $email = "citizen.test.$(Get-Random)@test.com"
    $script:citizenEmail = $email
    
    $body = @{
        email = $email
        password = "Test@123"
        fullName = "Test Citizen $(Get-Random)"
        phone = "0909111111"
        role = 0
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/Auth/register" `
        -Method POST `
        -Body $body `
        -ContentType "application/json"
    
    if ($response.success -ne $true) {
        throw "Registration failed: $($response.message)"
    }
    
    "   User ID: $($response.data.userId)`n   Email: $($response.data.email)`n   Role: $($response.data.role)`n   Token: $($response.data.token.Substring(0, 50))..."
}

# ============================================================
# TEST 4: POST /api/Auth/register - Collector (Production Way)
# ============================================================
$collectorEmail = $null

Test-Endpoint -Name "POST /api/Auth/register - Register Collector with dynamic teamId" -Test {
    if ($null -eq $firstTeamId) {
        throw "No team ID available"
    }
    
    $email = "collector.test.$(Get-Random)@test.com"
    $script:collectorEmail = $email
    
    $body = @{
        email = $email
        password = "Test@123"
        fullName = "Test Collector $(Get-Random)"
        phone = "0909222222"
        role = 1
        teamId = $firstTeamId  # ✅ Dynamic from API
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/Auth/register" `
        -Method POST `
        -Body $body `
        -ContentType "application/json"
    
    if ($response.success -ne $true) {
        throw "Registration failed: $($response.message)"
    }
    
    "   User ID: $($response.data.userId)`n   Email: $($response.data.email)`n   Role: $($response.data.role)`n   Team ID: $firstTeamId (from API ✅)`n   Token: $($response.data.token.Substring(0, 50))..."
}

# ============================================================
# TEST 5: POST /api/Auth/register - Enterprise
# ============================================================
$enterpriseEmail = $null

Test-Endpoint -Name "POST /api/Auth/register - Register Enterprise" -Test {
    $email = "enterprise.test.$(Get-Random)@test.com"
    $script:enterpriseEmail = $email
    
    $body = @{
        email = $email
        password = "Test@123"
        fullName = "Test Enterprise $(Get-Random)"
        phone = "0909333333"
        role = 2
        districtId = 1
        wasteTypes = "Plastic,Paper"  # ✅ String (comma-separated), not array
        dailyCapacity = 500
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/Auth/register" `
        -Method POST `
        -Body $body `
        -ContentType "application/json"
    
    if ($response.success -ne $true) {
        throw "Registration failed: $($response.message)"
    }
    
    "   User ID: $($response.data.userId)`n   Email: $($response.data.email)`n   Role: $($response.data.role)`n   Token: $($response.data.token.Substring(0, 50))..."
}

# ============================================================
# TEST 6: POST /api/Auth/login - Citizen
# ============================================================
Test-Endpoint -Name "POST /api/Auth/login - Login as Citizen" -Test {
    if ($null -eq $citizenEmail) {
        throw "No citizen email available"
    }
    
    $body = @{
        email = $citizenEmail
        password = "Test@123"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/Auth/login" `
        -Method POST `
        -Body $body `
        -ContentType "application/json"
    
    if ($response.success -ne $true) {
        throw "Login failed: $($response.message)"
    }
    
    "   User ID: $($response.data.userId)`n   Email: $($response.data.email)`n   Role: $($response.data.role)`n   Token: $($response.data.token.Substring(0, 50))...`n   Expires: $($response.data.expiresAt)"
}

# ============================================================
# TEST 7: POST /api/Auth/login - Collector
# ============================================================
Test-Endpoint -Name "POST /api/Auth/login - Login as Collector" -Test {
    if ($null -eq $collectorEmail) {
        throw "No collector email available"
    }
    
    $body = @{
        email = $collectorEmail
        password = "Test@123"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/Auth/login" `
        -Method POST `
        -Body $body `
        -ContentType "application/json"
    
    if ($response.success -ne $true) {
        throw "Login failed: $($response.message)"
    }
    
    "   User ID: $($response.data.userId)`n   Email: $($response.data.email)`n   Role: $($response.data.role)`n   Token: $($response.data.token.Substring(0, 50))..."
}

# ============================================================
# SUMMARY
# ============================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Yellow" })
Write-Host "         TEST SUMMARY" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Yellow" })
Write-Host "========================================" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Yellow" })
Write-Host ""
Write-Host "Total Tests: $($testsPassed + $testsFailed)" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Yellow" })
Write-Host "Passed: $testsPassed" -ForegroundColor Green
if ($testsFailed -gt 0) {
    Write-Host "Failed: $testsFailed" -ForegroundColor Red
}
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "ALL TESTS PASSED! PRODUCTION-READY!" -ForegroundColor Green
} else {
    Write-Host "SOME TESTS FAILED - CHECK ABOVE" -ForegroundColor Yellow
}

Write-Host "========================================" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Yellow" })
Write-Host ""

# Detailed results
if ($testsFailed -gt 0) {
    Write-Host "Failed Tests:" -ForegroundColor Red
    $testResults | Where-Object { $_.Status -eq "FAILED" } | ForEach-Object {
        Write-Host "  ❌ $($_.Test)" -ForegroundColor Red
        Write-Host "     $($_.Details)" -ForegroundColor Gray
    }
    Write-Host ""
}

# Exit code
if ($testsFailed -gt 0) {
    exit 1
} else {
    exit 0
}
