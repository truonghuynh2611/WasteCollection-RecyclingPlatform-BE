# 🐛 BÁO CÁO LỖI - REFRESHTOKEN FOREIGN KEY CONSTRAINT

## 📋 THÔNG TIN HỆ THỐNG
- **Project:** Waste Collection Platform Backend (.NET 8 + PostgreSQL)
- **ORM:** Entity Framework Core với Npgsql
- **Database:** PostgreSQL
- **Lỗi xuất hiện:** Khi đăng ký user mới (POST /api/auth/register)

---

## ❌ LỖI HIỆN TẠI

### **Mô tả lỗi:**
Khi người dùng đăng ký tài khoản (Citizen/Collector/Enterprise), hệ thống báo lỗi:

```json
{
  "success": false,
  "message": "An unexpected error occurred. Please try again later.",
  "data": null,
  "errors": [],
  "timestamp": "2026-03-12T13:34:17.619548Z"
}
```

### **Log chi tiết từ server:**
```
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes.
---> Npgsql.PostgresException (0x80004005): 23503: insert or update on table "refreshtoken" 
violates foreign key constraint "fk_refreshtoken_user"

DETAIL: Detail redacted as it may contain sensitive data.

Exception data:
  Severity: ERROR
  SqlState: 23503
  MessageText: insert or update on table "refreshtoken" violates foreign key constraint "fk_refreshtoken_user"
  SchemaName: public
  TableName: refreshtoken
  ConstraintName: fk_refreshtoken_user
```

### **SQL được thực thi:**
```sql
-- Bước 1: Tạo User - THÀNH CÔNG ✅
INSERT INTO "Users" ("Email", "FullName", "Password", "Phone", "ResetPasswordToken", 
                     "ResetTokenExpiry", "Role", "Status", "VerificationToken", "VerificationTokenExpiry")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)
RETURNING "UserId", "EmailVerified";

-- Bước 2: Tạo Citizen - THÀNH CÔNG ✅
INSERT INTO "Citizens" ("TotalPoints", "UserId")
VALUES (@p0, @p1)
RETURNING "CitizenId";

-- Bước 3: Tạo RefreshToken - THẤT BẠI ❌
INSERT INTO refreshtoken (createdat, expiresat, isrevoked, revokedat, token, userid)
VALUES (@p2, @p3, @p4, @p5, @p6, @p7)
RETURNING refreshtokenid;
-- LỖI: Foreign key constraint "fk_refreshtoken_user" bị vi phạm
```

---

## 🔍 PHÂN TÍCH VẤN ĐỀ

### **1. Database Schema Hiện Tại**

#### **Bảng Users:**
```sql
CREATE TABLE "Users" (
    "UserId" SERIAL PRIMARY KEY,  -- ✅ Đúng tên cột
    "Email" VARCHAR(100) NOT NULL,
    "FullName" VARCHAR(100) NOT NULL,
    -- ... các cột khác
);
```

#### **Bảng refreshtoken (Migration SQL):**
```sql
CREATE TABLE IF NOT EXISTS refreshtoken (
    refreshtokenid SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL,
    token VARCHAR(500) NOT NULL UNIQUE,
    expiresat TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    isrevoked BOOLEAN DEFAULT FALSE,
    revokedat TIMESTAMP WITHOUT TIME ZONE,
    
    -- ❌ LỖI Ở ĐÂY: Tên bảng SAI
    CONSTRAINT fk_refreshtoken_user 
    FOREIGN KEY (userid) 
    REFERENCES \"User\"(userid)    -- ❌ Bảng "User" KHÔNG TỒN TẠI!
    ON DELETE CASCADE
);
```

**VẤN ĐỀ:**
- Foreign key tham chiếu đến bảng `"User"` (sai)
- Nhưng bảng thật tên là `"Users"` (đúng)
- Cột trong bảng Users là `"UserId"` (PascalCase) nhưng FK tham chiếu `userid` (lowercase)

---

### **2. EF Core Configuration**

#### **File: RefreshTokenConfiguration.cs**
```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refreshtoken");  // ✅ Đúng

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.Userid)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refreshtoken_user");  // ✅ Đúng tên constraint
    }
}
```

#### **File: UserConfiguration.cs**
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");  // ✅ Tên bảng đúng
        
        builder.HasKey(e => e.Userid)
            .HasName("users_pkey");
            
        builder.Property(e => e.Userid)
            .HasColumnName("UserId")  // ✅ Cột tên UserId
            .UseIdentityAlwaysColumn();
    }
}
```

---

### **3. Entity Models**

#### **RefreshToken.cs:**
```csharp
[Table("refreshtoken")]
public class RefreshToken
{
    [Key]
    [Column("refreshtokenid")]
    public int Refreshtokenid { get; set; }

    [Required]
    [Column("userid")]
    public int Userid { get; set; }  // ✅ FK field

    [ForeignKey(nameof(Userid))]
    public virtual User User { get; set; } = null!;  // ✅ Navigation property
}
```

#### **User.cs:**
```csharp
public partial class User
{
    public int Userid { get; set; }  // Maps to column "UserId"
    // ... other properties
}
```

---

### **4. Business Logic (AuthService.cs)**

```csharp
public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        // Bước 1: Tạo User
        var user = new User { ... };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();  // ✅ Get UserId
        
        // Bước 2: Tạo role-specific entity (Citizen/Collector/Enterprise)
        switch (request.Role)
        {
            case UserRole.Citizen:
                var citizen = new Citizen { Userid = user.Userid };
                await _unitOfWork.Citizens.AddAsync(citizen);
                break;
            // ...
        }
        
        // Bước 3: Tạo RefreshToken
        var refreshTokenEntity = new RefreshToken
        {
            Userid = user.Userid,  // ✅ Có UserId hợp lệ
            Token = refreshToken,
            Expiresat = refreshTokenExpiration,
            Createdat = DateTime.UtcNow,
            Isrevoked = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        
        // Bước 4: Save tất cả trong transaction
        await _unitOfWork.SaveChangesAsync();  // ❌ LỖI Ở ĐÂY
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

## 🎯 NGUYÊN NHÂN GỐC RỄ

**Foreign Key Constraint trong database SAI:**

```sql
-- ❌ HIỆN TẠI (SAI):
CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) 
REFERENCES \"User\"(userid)  -- Bảng "User" không tồn tại!

-- ✅ CẦN PHẢI LÀ:
CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) 
REFERENCES "Users"("UserId")  -- Bảng "Users" với cột "UserId"
```

**Chi tiết:**
1. Migration SQL tạo FK tham chiếu đến bảng `"User"` (sai)
2. Nhưng EF Core đã tạo bảng tên `"Users"` (đúng)
3. Khi INSERT vào `refreshtoken`, PostgreSQL không tìm thấy bảng `"User"` để validate FK
4. → Foreign key constraint failed

---

## 💡 GIẢI PHÁP ĐỀ XUẤT

### **Giải pháp 1: Sửa Foreign Key Constraint trong Database**

```sql
-- Bước 1: Drop constraint cũ
ALTER TABLE refreshtoken 
DROP CONSTRAINT IF EXISTS fk_refreshtoken_user;

-- Bước 2: Tạo lại constraint đúng
ALTER TABLE refreshtoken
ADD CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) 
REFERENCES "Users"("UserId") 
ON DELETE CASCADE;
```

**Cách thực hiện:**
```powershell
# Option 1: psql command line
$env:PGPASSWORD="123456"
psql -h localhost -p 5432 -U postgres -d waste_management -f fix_refreshtoken_fk.sql

# Option 2: pgAdmin/DBeaver
# - Mở database waste_management
# - Execute SQL trên
```

---

### **Giải pháp 2: Kiểm tra lại tên bảng và cột**

Chạy query để verify:

```sql
-- Kiểm tra tên bảng Users
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name LIKE '%ser%';

-- Kiểm tra cột UserId
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'Users';

-- Kiểm tra constraint hiện tại
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name = 'refreshtoken';
```

---

### **Giải pháp 3: Sửa Migration SQL (để tránh lỗi lần sau)**

**File: `refresh_token_migration.sql`**

```sql
-- TRƯỚC (SAI):
CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) 
REFERENCES \"User\"(userid) ON DELETE CASCADE

-- SAU (ĐÚNG):
CONSTRAINT fk_refreshtoken_user 
FOREIGN KEY (userid) 
REFERENCES "Users"("UserId") ON DELETE CASCADE
```

---

## 📝 REQUEST DATA ĐÃ TEST

```json
{
  "fullName": "Tran Thi Uyen Nhi",
  "email": "Vannhattran35@gmail.com",
  "password": "Testnumber1@123",
  "phone": "0946584167",
  "role": 0
}
```

**Kết quả:** 
- User được tạo thành công trong bảng "Users"
- Citizen được tạo thành công trong bảng "Citizens"
- ❌ RefreshToken FAILED với FK constraint error

---

## 🔧 THÔNG TIN BỔ SUNG

### **Connection String:**
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=waste_management;Username=postgres;Password=123456"
```

### **PostgreSQL Version:** (cần kiểm tra)
```sql
SELECT version();
```

### **EF Core Version:**
- Microsoft.EntityFrameworkCore: 8.x
- Npgsql.EntityFrameworkCore.PostgreSQL: 8.x

---

## ❓ CÂU HỎI CHO CHATGPT

1. **Giải pháp nào tối ưu nhất để fix lỗi này?**
   - Sửa constraint trong database?
   - Recreate lại migration?
   - Cả hai?

2. **Có cần thay đổi EF Core configuration không?**

3. **Làm sao để tránh lỗi tương tự trong tương lai?**
   - Naming convention cho PostgreSQL + EF Core?
   - Best practices?

4. **Có cách nào để verify FK constraint đúng sau khi fix không?**

5. **Có ảnh hưởng gì đến data đã có trong database không?**

---

## 📊 FILES LIÊN QUAN

1. `WasteCollectionPlatform.Business/Services/Implementations/AuthService.cs` (line 199)
2. `WasteCollectionPlatform.DataAccess/Entities/RefreshToken.cs`
3. `WasteCollectionPlatform.DataAccess/Entities/User.cs`
4. `WasteCollectionPlatform.DataAccess/Configurations/RefreshTokenConfiguration.cs`
5. `WasteCollectionPlatform.DataAccess/Configurations/UserConfiguration.cs`
6. `refresh_token_migration.sql` (line 10)

---

**END OF REPORT**
