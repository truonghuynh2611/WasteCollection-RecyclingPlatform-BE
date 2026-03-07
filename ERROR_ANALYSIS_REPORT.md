# 🔴 BÁO CÁO PHÂN TÍCH LỖI CHI TIẾT - WASTE COLLECTION PLATFORM

## 📋 TỔNG QUAN CÁC LỖI

Hệ thống có **3 LỖI NGHIÊM TRỌNG** cần khắc phục:

1. ❌ **Collector Registration** - FAILED
2. ❌ **Enterprise Registration** - FAILED  
3. ❌ **Admin Refresh Token** - Shows "Refresh token has been revoked"

---

## 🔍 PHÂN TÍCH CHI TIẾT TỪNG LỖI

### ❌ LỖI 1: ENTERPRISE REGISTRATION FAILED

#### **Triệu chứng:**
- Enterprise đăng ký bị lỗi ngay tại endpoint `/api/Auth/register`
- Lỗi từ database: Foreign Key Constraint Violation

#### **Nguyên nhân gốc rễ:**
```
PostgresException: 23503
insert or update on table "enterprise" violates foreign key constraint "fk_enterprise_district"
```

**Giải thích:**
- Enterprise entity có field `districtid` (nullable INTEGER)
- Field này là FOREIGN KEY reference đến bảng `district(districtid)`
- Khi user đăng ký Enterprise với `districtId` (ví dụ: 1, 2, 3...), hệ thống kiểm tra xem district đó có tồn tại không
- **VẤN ĐỀ:** Bảng `district` HOÀN TOÀN TRỐNG - không có dữ liệu nào!
- PostgreSQL từ chối INSERT vì vi phạm ràng buộc tham chiếu

#### **Vị trí lỗi trong code:**

1. **AuthService.cs** (line 158-172):
```csharp
case UserRole.Enterprise:
    var enterprise = new Enterprise
    {
        Userid = user.Userid,
        Districtid = request.DistrictId,  // ⚠️ LỖI NGAY ĐÂY
        Wastetypes = request.WasteTypes,
        Dailycapacity = request.DailyCapacity ?? 100,
        Currentload = 0,
        Status = true
    };
    await _unitOfWork.Enterprises.AddAsync(enterprise);
    break;
```

2. **Enterprise table schema** (Program.cs line 167-176):
```sql
CREATE TABLE IF NOT EXISTS enterprise (
    enterpriseid SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL UNIQUE,
    districtid INTEGER,  -- ⚠️ NULLABLE nhưng có FK constraint
    wastetypes VARCHAR(255),
    dailycapacity INTEGER,
    currentload INTEGER DEFAULT 0,
    status BOOLEAN DEFAULT TRUE,
    CONSTRAINT fk_enterprise_user FOREIGN KEY (userid) REFERENCES "User"(userid) ON DELETE CASCADE,
    CONSTRAINT fk_enterprise_district FOREIGN KEY (districtid) 
        REFERENCES district(districtid) ON DELETE SET NULL  -- ⚠️ FK nhưng district table TRỐNG
);
```

#### **Giải pháp:**

**OPTION 1 (KHUYẾN NGHỊ): Seed dữ liệu District**
```sql
-- Thêm vào Program.cs hoặc tạo migration script
INSERT INTO district (districtname, latitude, longitude) VALUES
('Quận 1', 10.7769, 106.7009),
('Quận 2', 10.7829, 106.7428),
('Quận 3', 10.7860, 106.6897),
('Quận 4', 10.7574, 106.7053),
('Quận 5', 10.7567, 106.6676),
('Quận 6', 10.7486, 106.6352),
('Quận 7', 10.7333, 106.7196),
('Quận 8', 10.7383, 106.6755),
('Quận 9', 10.8351, 106.7803),
('Quận 10', 10.7719, 106.6685),
('Quận 11', 10.7629, 106.6509),
('Quận 12', 10.8626, 106.6772),
('Bình Thạnh', 10.8075, 106.7047),
('Tân Bình', 10.7995, 106.6530),
('Tân Phú', 10.7846, 106.6257),
('Phú Nhuận', 10.7978, 106.6838),
('Bình Tân', 10.7407, 106.6026),
('Gò Vấp', 10.8401, 106.6668),
('Thủ Đức', 10.8509, 106.7713),
('Củ Chi', 10.9748, 106.4923),
('Hóc Môn', 10.8829, 106.5934),
('Bình Chánh', 10.7353, 106.5967),
('Nhà Bè', 10.6991, 106.7239),
('Cần Giờ', 10.4141, 106.9531);
ON CONFLICT DO NOTHING;
```

**OPTION 2: Cho phép districtid = NULL**
```csharp
// Trong AuthService.cs - RegisterAsync
case UserRole.Enterprise:
    var enterprise = new Enterprise
    {
        Userid = user.Userid,
        Districtid = null,  // ⬅️ Để NULL khi đăng ký, update sau
        Wastetypes = request.WasteTypes,
        Dailycapacity = request.DailyCapacity ?? 100,
        Currentload = 0,
        Status = true
    };
    await _unitOfWork.Enterprises.AddAsync(enterprise);
    break;
```

---

### ❌ LỖI 2: COLLECTOR REGISTRATION FAILED

#### **Triệu chứng:**
- Collector đăng ký bị lỗi ngay tại endpoint `/api/Auth/register`
- Tương tự Enterprise - lỗi Foreign Key Constraint

#### **Nguyên nhân gốc rễ:**
```
insert or update on table "collector" violates foreign key constraint "fk_collector_team"
```

**Giải thích:**
- Collector entity có field `teamid` (NOT NULL INTEGER)
- Field này là FOREIGN KEY reference đến bảng `team(teamid)`
- **VẤN ĐỀ:** Bảng `team` HOÀN TOÀN TRỐNG - không có dữ liệu nào!
- PostgreSQL từ chối INSERT vì vi phạm ràng buộc tham chiếu

#### **Vị trí lỗi trong code:**

**AuthService.cs** (line 145-157):
```csharp
case UserRole.Collector:
    if (!request.TeamId.HasValue)
    {
        throw new BusinessRuleException("Team ID is required for Collector registration.");
    }
    
    var collector = new Collector
    {
        Userid = user.Userid,
        Teamid = request.TeamId.Value,  // ⚠️ LỖI NGAY ĐÂY - team không tồn tại
        Status = true,
        Currenttaskcount = 0
    };
    await _unitOfWork.Collectors.AddAsync(collector);
    break;
```

#### **Giải pháp:**

**OPTION 1 (KHUYẾN NGHỊ): Seed dữ liệu Team**
```sql
-- Thêm vào Program.cs hoặc tạo migration script
INSERT INTO team (teamname, maxmembers, currentmembers) VALUES
('Team Alpha', 10, 0),
('Team Beta', 10, 0),
('Team Gamma', 10, 0),
('Team Delta', 10, 0),
('Team Epsilon', 10, 0),
('Team Zeta', 15, 0),
('Team Eta', 15, 0),
('Team Theta', 15, 0),
('Team North', 12, 0),
('Team South', 12, 0),
('Team East', 12, 0),
('Team West', 12, 0)
ON CONFLICT DO NOTHING;
```

**OPTION 2: Tạo team mặc định nếu không tồn tại**
```csharp
case UserRole.Collector:
    if (!request.TeamId.HasValue)
    {
        throw new BusinessRuleException("Team ID is required for Collector registration.");
    }
    
    // Validate team exists
    var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
    if (team == null)
    {
        throw new BusinessRuleException($"Team with ID {request.TeamId.Value} does not exist. Please select a valid team.");
    }
    
    var collector = new Collector
    {
        Userid = user.Userid,
        Teamid = request.TeamId.Value,
        Status = true,
        Currenttaskcount = 0
    };
    await _unitOfWork.Collectors.AddAsync(collector);
    break;
```

---

### ❌ LỖI 3: ADMIN REFRESH TOKEN "REVOKED"

#### **Triệu chứng:**
```
WasteCollectionPlatform.Common.Exceptions.UnauthorizedException: 
Refresh token has been revoked.
```

#### **Nguyên nhân gốc rễ:**

**Refresh Token là SINGLE-USE (Chỉ dùng 1 lần)**

**Luồng hoạt động đúng:**
```
1. User login → Nhận access_token + refresh_token_A
2. Access token hết hạn (60 phút)
3. User gọi /refresh với refresh_token_A 
   → Server:
      a) Validate refresh_token_A (OK)
      b) Mark refresh_token_A as REVOKED (isrevoked = TRUE) ⚠️
      c) Tạo refresh_token_B MỚI
      d) Trả về access_token mới + refresh_token_B
4. Lần sau access token hết hạn → User PHẢI dùng refresh_token_B
5. Nếu user vẫn dùng refresh_token_A (đã revoked) → ❌ LỖI "Refresh token has been revoked"
```

#### **Vị trí logic trong code:**

**AuthService.cs - RefreshTokenAsync** (line 238-240):
```csharp
// Check if token is revoked
if (refreshTokenEntity.Isrevoked == true)
{
    throw new UnauthorizedException("Refresh token has been revoked.");
}

// ... later ...

// Revoke old refresh token (line 255-257)
refreshTokenEntity.Isrevoked = true;  // ⬅️ MARK AS REVOKED
refreshTokenEntity.Revokedat = DateTime.UtcNow;
await _unitOfWork.SaveChangesAsync();

// Generate NEW refresh token
var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
```

#### **Tại sao user gặp lỗi này?**

**Có 2 khả năng:**

**KHẢ NĂNG 1: User đang dùng token cũ**
- Admin đã login và nhận refresh_token_OLD
- Admin gọi /refresh với refresh_token_OLD → Thành công, nhận refresh_token_NEW
- Admin QUÊN không lưu refresh_token_NEW
- Admin tiếp tục dùng refresh_token_OLD → ❌ LỖI (vì đã bị revoke)

**KHẢ NĂNG 2: Database bị duplicate requests**
- Client gửi 2 requests /refresh cùng lúc với cùng 1 token
- Request 1: OK → revoke token
- Request 2: FAILED → token đã bị revoke

#### **Giải pháp:**

**GIẢI PHÁP 1: User phải RE-LOGIN**
```
Admin cần login lại để nhận refresh token mới:
POST /api/Auth/login
{
  "email": "admin@example.com",
  "password": "Admin@123"
}

Response sẽ có:
{
  "data": {
    "token": "...",
    "refreshToken": "xyz123..."  ⬅️ LƯU TOKEN NÀY
  }
}
```

**GIẢI PHÁP 2: Client phải lưu refresh token mới sau mỗi lần refresh**
```javascript
// ĐÚNG:
let refreshToken = "stored_token";

async function refreshAccessToken() {
  const response = await fetch('/api/Auth/refresh', {
    method: 'POST',
    body: JSON.stringify({ refreshToken })
  });
  
  const data = await response.json();
  
  // ⬇️ QUAN TRỌNG: Lưu refresh token MỚI
  refreshToken = data.data.refreshToken;
  localStorage.setItem('refresh_token', data.data.refreshToken);
  
  return data.data.token;
}
```

---

## 🔧 CRITICAL MISSING: REFRESHTOKENREPOSITORY KHÔNG ĐƯỢC ĐĂNG KÝ TRONG DI CONTAINER

### **Vấn đề:**

**Program.cs** (line 114-124) - Repository Registration:
```csharp
// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICitizenRepository, CitizenRepository>();
builder.Services.AddScoped<ICollectorRepository, CollectorRepository>();
builder.Services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
// ⚠️ THIẾU: builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### **Tại sao code vẫn chạy được?**

UnitOfWork sử dụng **lazy initialization**:
```csharp
// UnitOfWork.cs (line 59-60)
public IRefreshTokenRepository RefreshTokens => 
    _refreshTokens ??= new RefreshTokenRepository(_context);
```

Code này tạo instance trực tiếp **NGOÀI DI container** → Hoạt động nhưng:
- ❌ Không tuân thủ Dependency Injection pattern
- ❌ Khó test (không thể mock)
- ❌ Có thể gây memory leak trong một số trường hợp
- ❌ Không nhất quán với các repository khác

### **Giải pháp:**

**Thêm dòng sau vào Program.cs (sau line 123):**
```csharp
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
```

---

## 📊 TỔNG KẾT - CHECKLIST KHẮC PHỤC

### ✅ **BẮT BUỘC PHẢI LÀM:**

1. **[ ] Seed dữ liệu District vào database**
   - Tạo migration script hoặc thêm vào Program.cs
   - INSERT ít nhất 10-20 districts

2. **[ ] Seed dữ liệu Team vào database**
   - Tạo migration script hoặc thêm vào Program.cs
   - INSERT ít nhất 5-10 teams

3. **[ ] Đăng ký RefreshTokenRepository trong DI container**
   - Thêm `AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()` vào Program.cs

4. **[ ] Hướng dẫn user về Refresh Token lifecycle**
   - Giải thích refresh token là single-use
   - Client phải lưu token mới sau mỗi lần refresh
   - Re-login nếu token bị revoked

### 🔍 **NÊN LÀM (Tăng trải nghiệm):**

5. **[ ] Validation rõ ràng hơn**
   - Kiểm tra team exists trước khi tạo Collector
   - Kiểm tra district exists trước khi tạo Enterprise
   - Trả về error message cụ thể: "District ID 5 does not exist"

6. **[ ] Cải thiện error handling**
   - Catch Foreign Key Exception riêng
   - Trả về message thân thiện thay vì raw SQL error

7. **[ ] Tạo endpoint GET /api/teams và GET /api/districts**
   - Cho phép client lấy danh sách teams/districts có sẵn
   - Hiển thị dropdown trong UI đăng ký

---

## 🧪 TEST CASES SAU KHI SỬA

### Test 1: Enterprise Registration
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "enterprise1@test.com",
  "password": "Test@123",
  "fullName": "Enterprise Company",
  "phone": "0901234567",
  "role": 2,
  "districtId": 1,  // ⬅️ PHẢI TỒN TẠI trong bảng district
  "wasteTypes": "Plastic,Paper,Metal",
  "dailyCapacity": 500
}
```

**Expected:** HTTP 201 Created với access_token + refresh_token

### Test 2: Collector Registration
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "collector1@test.com",
  "password": "Test@123",
  "fullName": "John Collector",
  "phone": "0907654321",
  "role": 1,
  "teamId": 1  // ⬅️ PHẢI TỒN TẠI trong bảng team
}
```

**Expected:** HTTP 201 Created với access_token + refresh_token

### Test 3: Refresh Token Flow
```http
# Step 1: Login
POST /api/Auth/login
{
  "email": "admin@test.com",
  "password": "Admin@123"
}

# Response:
{
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "abc123xyz"  ⬅️ LƯU TOKEN NÀY
  }
}

# Step 2: Refresh (sau 60 phút)
POST /api/Auth/refresh
{
  "refreshToken": "abc123xyz"
}

# Response:
{
  "data": {
    "token": "eyJhbGc...",
    "refreshToken": "def456uvw"  ⬅️ TOKEN MỚI - THAY THẾ TOKEN CŨ
  }
}

# Step 3: Refresh lần 2 (PHẢI dùng token mới)
POST /api/Auth/refresh
{
  "refreshToken": "def456uvw"  ⬅️ DÙNG TOKEN MỚI, KHÔNG PHẢI "abc123xyz"
}
```

**Expected:** Mỗi lần refresh đều thành công và trả về token mới

---

## 📝 IMPLEMENTATION CODE SAMPLES

### Sample 1: Seed Districts in Program.cs

Thêm vào **Program.cs** sau dòng 182 (sau enterprise table migration):

```csharp
// Seed Districts
try
{
    var districtCount = await context.Database.ExecuteSqlRawAsync(@"
        INSERT INTO district (districtname, latitude, longitude) 
        SELECT * FROM (VALUES
            ('Quận 1', 10.7769, 106.7009),
            ('Quận 2', 10.7829, 106.7428),
            ('Quận 3', 10.7860, 106.6897),
            ('Quận 4', 10.7574, 106.7053),
            ('Quận 5', 10.7567, 106.6676),
            ('Quận 6', 10.7486, 106.6352),
            ('Quận 7', 10.7333, 106.7196),
            ('Quận 8', 10.7383, 106.6755),
            ('Quận 9', 10.8351, 106.7803),
            ('Quận 10', 10.7719, 106.6685),
            ('Quận 11', 10.7629, 106.6509),
            ('Quận 12', 10.8626, 106.6772),
            ('Bình Thạnh', 10.8075, 106.7047),
            ('Tân Bình', 10.7995, 106.6530),
            ('Tân Phú', 10.7846, 106.6257),
            ('Phú Nhuận', 10.7978, 106.6838),
            ('Bình Tân', 10.7407, 106.6026),
            ('Gò Vấp', 10.8401, 106.6668),
            ('Thủ Đức', 10.8509, 106.7713),
            ('Củ Chi', 10.9748, 106.4923),
            ('Hóc Môn', 10.8829, 106.5934),
            ('Bình Chánh', 10.7353, 106.5967),
            ('Nhà Bè', 10.6991, 106.7239),
            ('Cần Giờ', 10.4141, 106.9531)
        ) AS v(districtname, latitude, longitude)
        WHERE NOT EXISTS (
            SELECT 1 FROM district WHERE districtname = v.districtname
        );
    ");
    Console.WriteLine($"✅ Districts seeded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  District seed failed (might already exist): {ex.Message}");
}
```

### Sample 2: Seed Teams in Program.cs

Thêm tiếp sau district seed:

```csharp
// Seed Teams
try
{
    await context.Database.ExecuteSqlRawAsync(@"
        INSERT INTO team (teamname, maxmembers, currentmembers) 
        SELECT * FROM (VALUES
            ('Team Alpha', 10, 0),
            ('Team Beta', 10, 0),
            ('Team Gamma', 10, 0),
            ('Team Delta', 10, 0),
            ('Team Epsilon', 10, 0),
            ('Team Zeta', 15, 0),
            ('Team Eta', 15, 0),
            ('Team Theta', 15, 0),
            ('Team North', 12, 0),
            ('Team South', 12, 0),
            ('Team East', 12, 0),
            ('Team West', 12, 0)
        ) AS v(teamname, maxmembers, currentmembers)
        WHERE NOT EXISTS (
            SELECT 1 FROM team WHERE teamname = v.teamname
        );
    ");
    Console.WriteLine($"✅ Teams seeded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Team seed failed (might already exist): {ex.Message}");
}
```

### Sample 3: Add RefreshTokenRepository to DI

**Program.cs** - Thêm sau line 123:

```csharp
builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // ⬅️ THÊM DÒNG NÀY
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Sample 4: Enhanced Team Validation in AuthService

**AuthService.cs - RegisterAsync** - Replace Collector case:

```csharp
case UserRole.Collector:
    if (!request.TeamId.HasValue)
    {
        throw new BusinessRuleException("Team ID is required for Collector registration.");
    }
    
    // Validate team exists - ENHANCED
    var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
    if (team == null)
    {
        throw new BusinessRuleException(
            $"Team with ID {request.TeamId.Value} does not exist. " +
            "Please contact administrator to get valid team IDs."
        );
    }
    
    var collector = new Collector
    {
        Userid = user.Userid,
        Teamid = request.TeamId.Value,
        Status = true,
        Currenttaskcount = 0
    };
    await _unitOfWork.Collectors.AddAsync(collector);
    break;
```

---

## 🎯 KẾT LUẬN

**3 lỗi chính đều do THIẾU DỮ LIỆU SEED:**

1. ❌ Bảng `district` trống → Enterprise registration failed (FK constraint)
2. ❌ Bảng `team` trống → Collector registration failed (FK constraint)
3. ❌ Admin dùng refresh token cũ đã bị revoke → Refresh failed

**Ưu tiên khắc phục:**
1. **HIGH PRIORITY:** Seed district và team data
2. **MEDIUM PRIORITY:** Register RefreshTokenRepository trong DI
3. **LOW PRIORITY:** Enhanced validation và error messages

**Sau khi khắc phục 3 items trên, hệ thống sẽ hoạt động bình thường.**

---

**Báo cáo được tạo tự động bởi GitHub Copilot**  
**Thời gian phân tích:** Token analysis + Error log parsing + Code review  
**Trạng thái:** Ready for implementation
