# ✅ COLLECTOR REGISTRATION - PRODUCTION WAY
# Cách đúng: Query teams → Lấy ID thực tế → Register

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  📋 STEP 1: GET AVAILABLE TEAMS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

try {
    $teamsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/Team" -Method GET
    
    Write-Host "✅ Teams retrieved successfully!`n" -ForegroundColor Green
    Write-Host "Available Teams:" -ForegroundColor Yellow
    
    $teams = $teamsResponse.data
    foreach ($team in $teams) {
        Write-Host "  - ID: $($team.teamId) | Name: $($team.name) | AreaId: $($team.areaId)" -ForegroundColor White
    }
    
    # Get first available team ID
    $firstTeamId = $teams[0].teamId
    Write-Host "`n🎯 Using Team ID: $firstTeamId for registration" -ForegroundColor Green
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  📝 STEP 2: REGISTER COLLECTOR" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    $registerBody = @{
        email = "collector.dynamic@test.com"
        password = "Test@123"
        fullName = "Collector Dynamic Test"
        phone = "0909887766"
        role = 1
        teamId = $firstTeamId  # ✅ Dynamic team ID from API
    } | ConvertTo-Json
    
    Write-Host "Request Body:" -ForegroundColor Yellow
    $registerBody | Write-Host -ForegroundColor Gray
    
    Write-Host "`n⏳ Registering collector...`n" -ForegroundColor Yellow
    
    try {
        $registerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/register" -Method POST -Body $registerBody -ContentType "application/json"
        
        Write-Host "✅ SUCCESS! Collector registered!`n" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Yellow
        Write-Host "  UserId    : $($registerResponse.data.userId)" -ForegroundColor White
        Write-Host "  Email     : $($registerResponse.data.email)" -ForegroundColor White
        Write-Host "  FullName  : $($registerResponse.data.fullName)" -ForegroundColor White
        Write-Host "  Role      : $($registerResponse.data.role)" -ForegroundColor White
        Write-Host "  TeamId    : $firstTeamId" -ForegroundColor White
        Write-Host "  Token     : $($registerResponse.data.token.Substring(0,60))..." -ForegroundColor Gray
        
        Write-Host "`n========================================" -ForegroundColor Green
        Write-Host "  ✅ PRODUCTION-STYLE TEST PASSED!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Registration failed!`n" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            $_.ErrorDetails.Message | Write-Host -ForegroundColor Red
        } else {
            Write-Host $_.Exception.Message -ForegroundColor Red
        }
    }
    
    Write-Host "`n💡 This is the correct way:" -ForegroundColor Yellow
    Write-Host "   1. GET /api/Team → Lấy danh sách teams" -ForegroundColor Gray
    Write-Host "   2. Chọn teamId từ response" -ForegroundColor Gray
    Write-Host "   3. POST /api/Auth/register với teamId dynamic" -ForegroundColor Gray
    Write-Host "`n❌ NEVER hardcode teamId=1 in production!`n" -ForegroundColor Red
    
} catch {
    Write-Host "`n❌ ERROR getting teams!`n" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        $_.ErrorDetails.Message | Write-Host -ForegroundColor Red
    } else {
        Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}
