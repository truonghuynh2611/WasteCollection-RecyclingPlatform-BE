# 🧪 HƯỚNG DẪN TEST ĐẦY ĐỦ - WASTE COLLECTION PLATFORM API

## ✅ TRẠNG THÁI
- **Database:** Fixed (FK refreshtoken → Users.UserId đã đúng)
- **Data:** Cleaned (TRUNCATE refreshtoken)
- **App:** Running at http://localhost:5000
- **Swagger UI:** http://localhost:5000/swagger

---

## 📋 DANH SÁCH API CÓ THỂ TEST (13 endpoints)

### **🔐 AUTHENTICATION (6 endpoints)**
1. POST /api/auth/register - Đăng ký tài khoản
2. POST /api/auth/login - Đăng nhập
3. POST /api/auth/refresh-token - Làm mới token
4. GET /api/auth/verify-email - Xác thực email
5. POST /api/auth/forgot-password - Quên mật khẩu
6. POST /api/auth/reset-password - Đặt lại mật khẩu

### **👥 TEAM (2 endpoints)**
7. GET /api/team - Lấy danh sách teams
8. GET /api/team/{id} - Lấy team theo ID

### **👤 CITIZEN (3 endpoints)**
9. GET /api/citizen - Lấy danh sách citizens
10. GET /api/citizen/{id} - Lấy citizen theo ID
11. GET /api/citizen/stats - Thống kê citizens

### **🗺️ DISTRICT (2 endpoints)**
12. GET /api/district - Lấy danh sách districts
13. GET /api/district/{id} - Lấy district với areas

---

## 🚀 THỨ TỰ TEST KHUYẾN NGHỊ

### **BƯỚC 1: Test Team API (Không cần auth)**
Lấy teamId để dùng cho đăng ký Collector sau này.

#### **1.1. GET /api/team**
**Mục đích:** Lấy danh sách tất cả teams

**Cách test trên Swagger:**
1. Expand `GET /api/team`
2. Click "Try it out"
3. Click "Execute"

**Response mong đợi:**
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

**Lưu lại:** Chọn 1 teamId (ví dụ: 3) để dùng cho Bước 3.

---

#### **1.2. GET /api/team/{id}**
**Mục đích:** Lấy thông tin chi tiết 1 team

**Request:**
- Path parameter: `id = 3`

**Response mong đợi:**
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

### **BƯỚC 2: Test District API (Không cần auth)**

#### **2.1. GET /api/district**
**Mục đích:** Lấy danh sách districts

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Retrieved 10 districts successfully.",
  "data": [
    {
      "districtId": 1,
      "districtName": "District 1",
      "areasCount": 5
    }
  ]
}
```

---

#### **2.2. GET /api/district/{id}**
**Mục đích:** Lấy district với danh sách areas

**Request:**
- Path parameter: `id = 1`

**Response mong đợi:**
```json
{
  "success": true,
  "message": "District retrieved successfully.",
  "data": {
    "districtId": 1,
    "districtName": "District 1",
    "areas": [
      {
        "areaId": 1,
        "areaName": "Ward 1"
      }
    ]
  }
}
```

---

### **BƯỚC 3: Test Register - Citizen**
**Mục đích:** Đăng ký tài khoản Citizen (người dân bình thường)

#### **API:** POST /api/auth/register

**Request Body:**
```json
{
  "fullName": "Nguyen Van A",
  "email": "citizen1@test.com",
  "password": "Test@123",
  "phone": "0901234567",
  "role": 0
}
```

**LƯU Ý QUAN TRỌNG:**
- ✅ Chỉ cần 5 fields trên cho Citizen
- ❌ KHÔNG gửi: teamId, vehicleInfo, districtId, wasteTypes, dailyCapacity
- Role 0 = Citizen

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": 30,
    "email": "citizen1@test.com",
    "fullName": "Nguyen Van A",
    "role": "Citizen",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc-123-def...",
    "expiresAt": "2026-03-12T14:45:00Z",
    "status": true
  }
}
```

**✅ KIỂM TRA:**
- [x] Response có `success: true`
- [x] Có `userId` (ví dụ: 30)
- [x] Có `token` (JWT)
- [x] Có `refreshToken`
- [x] Role = "Citizen"

**LƯU LẠI:** Copy `token` để dùng cho các API cần auth

---

### **BƯỚC 4: Test Register - Collector**
**Mục đích:** Đăng ký nhân viên thu gom

#### **API:** POST /api/auth/register

**Request Body:**
```json
{
  "fullName": "Tran Van B",
  "email": "collector1@test.com",
  "password": "Test@123",
  "phone": "0902345678",
  "role": 1,
  "teamId": 3
}
```

**LƯU Ý:**
- ✅ Dùng `teamId` từ Bước 1 (ví dụ: 3)
- ✅ Role 1 = Collector
- ⚠️ `teamId` BẮT BUỘC cho Collector

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": 31,
    "email": "collector1@test.com",
    "fullName": "Tran Van B",
    "role": "Collector",
    "token": "eyJhbGc...",
    "refreshToken": "xyz-456...",
    "expiresAt": "2026-03-12T14:45:00Z",
    "status": true
  }
}
```

---

### **BƯỚC 5: Test Register - Enterprise**
**Mục đích:** Đăng ký doanh nghiệp thu gom

#### **API:** POST /api/auth/register

**Request Body:**
```json
{
  "fullName": "Cong ty ABC",
  "email": "enterprise1@test.com",
  "password": "Test@123",
  "phone": "0903456789",
  "role": 2,
  "districtId": 1,
  "wasteTypes": "Plastic,Paper",
  "dailyCapacity": 500
}
```

**LƯU Ý:**
- ✅ Role 2 = Enterprise
- ✅ `wasteTypes` là STRING (comma-separated), KHÔNG phải array
- ✅ `districtId` và `dailyCapacity` bắt buộc cho Enterprise

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": 32,
    "email": "enterprise1@test.com",
    "fullName": "Cong ty ABC",
    "role": "Enterprise",
    "token": "eyJhbGc...",
    "refreshToken": "qwe-789...",
    "expiresAt": "2026-03-12T14:45:00Z",
    "status": true
  }
}
```

---

### **BƯỚC 6: Test Login**
**Mục đích:** Đăng nhập với tài khoản đã tạo

#### **API:** POST /api/auth/login

**Request Body:**
```json
{
  "email": "citizen1@test.com",
  "password": "Test@123"
}
```

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "userId": 30,
    "email": "citizen1@test.com",
    "fullName": "Nguyen Van A",
    "role": "Citizen",
    "token": "eyJhbGc...",
    "refreshToken": "new-token-123...",
    "expiresAt": "2026-03-12T15:00:00Z",
    "status": true
  }
}
```

**LƯU LẠI:** Copy `token` mới

---

### **BƯỚC 7: Test Refresh Token**
**Mục đích:** Làm mới JWT token khi hết hạn

#### **API:** POST /api/auth/refresh-token

**Request Body:**
```json
{
  "token": "your-refresh-token-here"
}
```

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Token refreshed successfully.",
  "data": {
    "userId": 30,
    "email": "citizen1@test.com",
    "token": "new-jwt-token...",
    "refreshToken": "new-refresh-token...",
    "expiresAt": "2026-03-12T16:00:00Z"
  }
}
```

---

### **BƯỚC 8: Test Citizen APIs**

#### **8.1. GET /api/citizen**
**Mục đích:** Xem danh sách tất cả citizens

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Retrieved 1 citizens successfully.",
  "data": [
    {
      "citizenId": 1,
      "userId": 30,
      "email": "citizen1@test.com",
      "fullName": "Nguyen Van A",
      "phone": "0901234567",
      "totalPoints": 0,
      "status": true
    }
  ]
}
```

---

#### **8.2. GET /api/citizen/{id}**
**Mục đích:** Xem chi tiết 1 citizen

**Request:**
- Path parameter: `id = 30` (userId từ registration)

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Citizen retrieved successfully.",
  "data": {
    "citizenId": 1,
    "userId": 30,
    "email": "citizen1@test.com",
    "fullName": "Nguyen Van A",
    "phone": "0901234567",
    "totalPoints": 0,
    "status": true
  }
}
```

---

#### **8.3. GET /api/citizen/stats**
**Mục đích:** Xem thống kê tổng quan

**Response mong đợi:**
```json
{
  "success": true,
  "message": "Citizen statistics retrieved successfully.",
  "data": {
    "totalCitizens": 1,
    "activeCitizens": 1,
    "totalPoints": 0
  }
}
```

---

## 📊 BẢNG TỔNG HỢP ROLES

| Role Value | Role Name | Required Fields | Optional Fields |
|------------|-----------|-----------------|-----------------|
| 0 | Citizen | fullName, email, password, phone, role | - |
| 1 | Collector | fullName, email, password, phone, role, **teamId** | vehicleInfo |
| 2 | Enterprise | fullName, email, password, phone, role, **districtId**, **wasteTypes**, **dailyCapacity** | - |
| 3 | Admin | (Không test được qua register) | - |

---

## ❌ CÁC LỖI THƯỜNG GẶP VÀ CÁCH FIX

### **Lỗi 1: Email already exists**
```json
{
  "success": false,
  "message": "Email already exists.",
  "errors": ["Email already exists."]
}
```
**Fix:** Đổi email khác (ví dụ: citizen2@test.com)

---

### **Lỗi 2: Validation failed - Password**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Password must contain at least one uppercase, one lowercase, one number, and one special character."
  ]
}
```
**Fix:** Dùng password hợp lệ: `Test@123`

**Yêu cầu password:**
- Ít nhất 8 ký tự
- Có chữ hoa (A-Z)
- Có chữ thường (a-z)
- Có số (0-9)
- Có ký tự đặc biệt (@$!%*?&)

---

### **Lỗi 3: Team ID is required for Collector**
```json
{
  "success": false,
  "message": "Team ID is required for Collector registration.",
  "errors": ["Team ID is required for Collector registration."]
}
```
**Fix:** Thêm `teamId` vào request body

---

### **Lỗi 4: Invalid team specified**
```json
{
  "success": false,
  "message": "Invalid team specified.",
  "errors": ["Invalid team specified."]
}
```
**Fix:** Dùng teamId hợp lệ từ `GET /api/team`

---

## ✅ CHECKLIST TEST ĐẦY ĐỦ

- [ ] 1. GET /api/team (lấy teamId)
- [ ] 2. GET /api/team/{id}
- [ ] 3. GET /api/district
- [ ] 4. GET /api/district/{id}
- [ ] 5. POST /api/auth/register (Citizen)
- [ ] 6. POST /api/auth/register (Collector với teamId)
- [ ] 7. POST /api/auth/register (Enterprise)
- [ ] 8. POST /api/auth/login
- [ ] 9. POST /api/auth/refresh-token
- [ ] 10. GET /api/citizen
- [ ] 11. GET /api/citizen/{id}
- [ ] 12. GET /api/citizen/stats

---

## 🎯 MỤC TIÊU

✅ **Tất cả 13 endpoints phải test thành công**

Sau khi test xong, bạn sẽ có:
- 3 users đã đăng ký (Citizen, Collector, Enterprise)
- JWT tokens để dùng cho các API khác (khi implement thêm)
- Refresh tokens để làm mới JWT
- Hiểu rõ flow đăng ký và authentication

---

## 📝 GHI CHÚ

- **Email phải unique** - mỗi lần test lại phải đổi email
- **Token expires sau 60 phút** - cấu hình trong appsettings.json
- **Refresh token expires sau 30 ngày**
- **Swagger UI tự động format JSON** - dễ đọc và test

---

**Chúc bạn test thành công!** 🚀

Nếu gặp lỗi, kiểm tra:
1. App có đang chạy không? (http://localhost:5000)
2. Database có connect được không?
3. Request body có đúng format JSON không?
4. Email có bị trùng không?
