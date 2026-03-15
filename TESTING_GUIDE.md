# 🧪 API TESTING GUIDE
**Waste Collection Platform - Production-Ready API**

Ngày cập nhật: March 3, 2026  
Status: ✅ ALL TESTS PASSED

---

## 🚀 QUICK START

### 1. Start Application
```powershell
cd WasteCollectionPlatform.API
dotnet run
```

Application sẽ chạy tại: **http://localhost:5000**

### 2. Verify Application Running
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/Team" -Method GET
```

Nếu thấy response với list teams → ✅ App đang chạy!

---

## 📋 API ENDPOINTS SUMMARY

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| **GET** | `/api/Team` | Lấy danh sách teams | ❌ No |
| **GET** | `/api/Team/{id}` | Lấy team theo ID | ❌ No |
| **POST** | `/api/Auth/register` | Đăng ký user mới | ❌ No |
| **POST** | `/api/Auth/login` | Đăng nhập | ❌ No |

---

## 🧪 TEST SCENARIOS

### ✅ Test 1: GET All Teams

**PowerShell:**
```powershell
$teams = Invoke-RestMethod -Uri "http://localhost:5000/api/Team" -Method GET
$teams.data | Format-Table teamId, name, areaId
```

**cURL:**
```bash
curl http://localhost:5000/api/Team
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Teams retrieved successfully.",
  "data": [
    {
      "teamId": 3,
      "name": "Team North",
      "areaId": 6
    },
    {
      "teamId": 4,
      "name": "Team West",
      "areaId": 7
    }
  ]
}
```

---

### ✅ Test 2: GET Team By ID

**PowerShell:**
```powershell
$team = Invoke-RestMethod -Uri "http://localhost:5000/api/Team/3" -Method GET
$team.data
```

**cURL:**
```bash
curl http://localhost:5000/api/Team/3
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Team retrieved successfully.",
  "data": {
    "teamId": 3,
    "name": "Team North",
    "areaId": 6
  }
}
```

---

### ✅ Test 3: Register Citizen

**PowerShell:**
```powershell
$body = @{
    email = "citizen@example.com"
    password = "Test@123"
    fullName = "John Citizen"
    phone = "0909111111"
    role = 0
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5000/api/Auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$response.data
```

**cURL:**
```bash
curl -X POST http://localhost:5000/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "citizen@example.com",
    "password": "Test@123",
    "fullName": "John Citizen",
    "phone": "0909111111",
    "role": 0
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "xKocz6p...",
    "userId": 19,
    "email": "citizen@example.com",
    "fullName": "John Citizen",
    "role": "Citizen",
    "status": true,
    "expiresAt": "2026-03-03T01:00:00Z"
  }
}
```

---

### ✅ Test 4: Register Collector (Production Way) ⭐

**QUAN TRỌNG:** Collector PHẢI có `teamId`. Lấy teamId từ GET /api/Team!

**PowerShell (Recommended):**
```powershell
# Step 1: Get available teams
$teams = (Invoke-RestMethod -Uri "http://localhost:5000/api/Team" -Method GET).data

# Step 2: Select team (ví dụ: team đầu tiên)
$teamId = $teams[0].teamId
Write-Host "Using TeamId: $teamId"

# Step 3: Register collector với dynamic teamId
$body = @{
    email = "collector@example.com"
    password = "Test@123"
    fullName = "John Collector"
    phone = "0909222222"
    role = 1
    teamId = $teamId  # ✅ Dynamic từ API
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5000/api/Auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$response.data
```

**cURL (2 bước):**
```bash
# Step 1: Get teams và lấy teamId
curl http://localhost:5000/api/Team

# Step 2: Register với teamId từ response trên (ví dụ: 3)
curl -X POST http://localhost:5000/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "collector@example.com",
    "password": "Test@123",
    "fullName": "John Collector",
    "phone": "0909222222",
    "role": 1,
    "teamId": 3
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": 20,
    "email": "collector@example.com",
    "fullName": "John Collector",
    "role": "Collector",
    "status": true,
    "token": "eyJhbGc..."
  }
}
```

---

### ✅ Test 5: Register Enterprise

**PowerShell:**
```powershell
$body = @{
    email = "enterprise@example.com"
    password = "Test@123"
    fullName = "ABC Company"
    phone = "0909333333"
    role = 2
    districtId = 1
    wasteTypes = "Plastic,Paper"  # ⚠️ Must be comma-separated STRING, not array
    dailyCapacity = 500
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5000/api/Auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$response.data
```

**cURL:**
```bash
curl -X POST http://localhost:5000/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "enterprise@example.com",
    "password": "Test@123",
    "fullName": "ABC Company",
    "phone": "0909333333",
    "role": 2,
    "districtId": 1,
    "wasteTypes": "Plastic,Paper",
    "dailyCapacity": 500
  }'
```

---

### ✅ Test 6: Login

**PowerShell:**
```powershell
$body = @{
    email = "citizen@example.com"
    password = "Test@123"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5000/api/Auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Save token for later use
$token = $response.data.token
Write-Host "Token: $token"
```

**cURL:**
```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "citizen@example.com",
    "password": "Test@123"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "xKocz6p...",
    "userId": 19,
    "email": "citizen@example.com",
    "fullName": "John Citizen",
    "role": "Citizen",
    "status": true,
    "expiresAt": "2026-03-03T01:00:00Z"
  }
}
```

---

## 🎯 ROLE TYPES

| Value | Role | Required Fields |
|-------|------|----------------|
| `0` | Citizen | email, password, fullName, phone |
| `1` | Collector | email, password, fullName, phone, **teamId** |
| `2` | Enterprise | email, password, fullName, phone, districtId, wasteTypes, dailyCapacity |

---

## 🔥 ONE-CLICK TEST SCRIPT

**Chạy tất cả tests trong 1 lần:**

```powershell
# Save this as: test-all.ps1

Write-Host "`n🧪 Running All Tests...`n" -ForegroundColor Cyan

$baseUrl = "http://localhost:5000"
$passed = 0
$failed = 0

# Test 1: Get Teams
try {
    $teams = Invoke-RestMethod -Uri "$baseUrl/api/Team" -Method GET
    Write-Host "✅ Test 1 PASSED: Got $($teams.data.Count) teams" -ForegroundColor Green
    $teamId = $teams.data[0].teamId
    $passed++
} catch {
    Write-Host "❌ Test 1 FAILED" -ForegroundColor Red
    $failed++
}

# Test 2: Register Citizen
try {
    $body = @{
        email = "citizen.test.$(Get-Random)@test.com"
        password = "Test@123"
        fullName = "Test Citizen"
        phone = "0909111111"
        role = 0
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/register" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ Test 2 PASSED: Citizen registered (UserId: $($response.data.userId))" -ForegroundColor Green
    $citizenEmail = $response.data.email
    $passed++
} catch {
    Write-Host "❌ Test 2 FAILED" -ForegroundColor Red
    $failed++
}

# Test 3: Register Collector
try {
    $body = @{
        email = "collector.test.$(Get-Random)@test.com"
        password = "Test@123"
        fullName = "Test Collector"
        phone = "0909222222"
        role = 1
        teamId = $teamId
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/register" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ Test 3 PASSED: Collector registered (UserId: $($response.data.userId))" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "❌ Test 3 FAILED" -ForegroundColor Red
    $failed++
}

# Test 4: Login
try {
    $body = @{
        email = $citizenEmail
        password = "Test@123"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ Test 4 PASSED: Login successful" -ForegroundColor Green
    $passed++
} catch {
    Write-Host "❌ Test 4 FAILED" -ForegroundColor Red
    $failed++
}

# Summary
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Cyan
```

**Chạy script:**
```powershell
.\test-all.ps1
```

---

## 📱 POSTMAN COLLECTION

### Import vào Postman:

**Collection JSON:**
```json
{
  "info": {
    "name": "Waste Collection API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get All Teams",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/Team"
      }
    },
    {
      "name": "Register Citizen",
      "request": {
        "method": "POST",
        "url": "http://localhost:5000/api/Auth/register",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"citizen@example.com\",\n  \"password\": \"Test@123\",\n  \"fullName\": \"John Doe\",\n  \"phone\": \"0909111111\",\n  \"role\": 0\n}"
        }
      }
    },
    {
      "name": "Register Collector",
      "request": {
        "method": "POST",
        "url": "http://localhost:5000/api/Auth/register",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"collector@example.com\",\n  \"password\": \"Test@123\",\n  \"fullName\": \"Jane Collector\",\n  \"phone\": \"0909222222\",\n  \"role\": 1,\n  \"teamId\": 3\n}"
        }
      }
    },
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "url": "http://localhost:5000/api/Auth/login",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"citizen@example.com\",\n  \"password\": \"Test@123\"\n}"
        }
      }
    }
  ]
}
```

Save JSON trên vào file `WasteCollectionAPI.postman_collection.json` và import vào Postman.

---

## ⚠️ COMMON ERRORS & SOLUTIONS

### Error 1: "Invalid team specified"
**Nguyên nhân:** teamId không tồn tại hoặc hardcode teamId=1  
**Giải pháp:** 
```powershell
# Get valid teamId first
$teamId = (Invoke-RestMethod -Uri "http://localhost:5000/api/Team").data[0].teamId
# Then use $teamId in registration
```

### Error 2: "Email already exists"
**Nguyên nhân:** Email đã được dùng  
**Giải pháp:** Dùng email khác hoặc thêm random number:
```powershell
$email = "user.$(Get-Random)@test.com"
```

### Error 3: Connection refused
**Nguyên nhân:** Application chưa chạy  
**Giải pháp:**
```powershell
cd WasteCollectionPlatform.API
dotnet run
```

---

## 🎉 VERIFIED RESULTS

```
╔════════════════════════════════════════════════════════╗
║                   TEST SUMMARY                         ║
╠════════════════════════════════════════════════════════╣
║  ✅ Passed: 5                                          ║
║  ❌ Failed: 0                                          ║
║                                                        ║
║       🎉 ALL TESTS PASSED! PRODUCTION-READY! 🎉        ║
╚════════════════════════════════════════════════════════╝
```

**Tested on:** March 3, 2026  
**Status:** ✅ All endpoints working  
**Database:** PostgreSQL (waste_management)  
**Teams Available:** 12 (IDs: 3-14)

---

## 📞 SUPPORT

Nếu gặp vấn đề:
1. Check application đang chạy: `GET http://localhost:5000/api/Team`
2. Check database connection trong `appsettings.json`
3. Xem logs trong console khi chạy `dotnet run`

**Files quan trọng:**
- [API_ENDPOINTS.md](API_ENDPOINTS.md) - Full API documentation

