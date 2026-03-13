# 🔥 PHÂN TÍCH LỖI POSTGRESQL ENUM - GỬI CHO CHATGPT

## 📋 YÊU CẦU
Cần giải quyết lỗi TypeLoadException khi map PostgreSQL enum với EF Core + Npgsql.

---

## 🔴 LỖI HIỆN TẠI

### Error Message
```
System.TypeLoadException: Could not load type 'Npgsql.Internal.HackyEnumTypeMapping' 
from assembly 'Npgsql, Version=10.0.1.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7'.

Stack trace:
at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlTypeMappingSource
   .SetupEnumMappings(ISqlGenerationHelper sqlGenerationHelper, NpgsqlDataSource dataSource)
```

### Context
- **Endpoint**: `GET /api/Citizen`
- **HTTP Status**: 500 Internal Server Error
- **Operation**: Query Users table → Read "Role" column (type: user_role enum)

---

## 🔧 PACKAGE VERSIONS (VERSION CONFLICT DETECTED)

### WasteCollectionPlatform.DataAccess
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

### WasteCollectionPlatform.Common
```xml
<PackageReference Include="Npgsql" Version="10.0.1" />  ⚠️ CONFLICT
```

### Target Framework
`.NET 8.0`

### ⚠️ Incompatibility
- **Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0** expects **Npgsql 8.x**
- **Npgsql 10.0.1** installed (breaking change: `HackyEnumTypeMapping` removed)

---

## 🎯 C# CODE CONFIGURATION

### 1. Enum Definition
**File**: `WasteCollectionPlatform.Common/Enums/UserRole.cs`

```csharp
namespace WasteCollectionPlatform.Common.Enums;

public enum UserRole
{
    Citizen = 0,
    Collector = 1,
    Enterprise = 2,
    Admin = 3
}
```

### 2. Program.cs Configuration
**File**: `WasteCollectionPlatform.API/Program.cs` (Lines 63-73)

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString!);
// Map enum WITHOUT name translator (tried NpgsqlSnakeCaseNameTranslator - failed)
dataSourceBuilder.MapEnum<UserRole>("user_role");

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<WasteManagementContext>(options =>
    options.UseNpgsql(dataSource));
```

### 3. DbContext Configuration
**File**: `WasteCollectionPlatform.DataAccess/Context/WasteManagementContext.cs`

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder
        .HasPostgresEnum("image_type", new[] { "Citizen", "Collector" })
        .HasPostgresEnum("point_transaction_type", new[] { "Earn", "Redeem" })
        .HasPostgresEnum("report_status", new[] { "Pending", "Accepted", "Assigned", "OnTheWay", "Collected", "Failed" })
        .HasPostgresEnum("team_type", new[] { "Main", "Support" })
        .HasPostgresEnum<UserRole>("user_role");  // ✅ Added as per recommendation
    
    // ... entity configurations
}
```

---

## 📝 DEBUG HISTORY

1. **Initial Error**: `InvalidCastException: Received enum value 'Admin' from database which wasn't found on enum UserRole`
2. **Tried**: `NpgsqlSnakeCaseNameTranslator` → FAILED (expected 'admin', DB has 'Admin')
3. **Tried**: Remove translator, direct mapping → FAILED (same error)
4. **Tried**: Add `Npgsql 10.0.1` package to Common project → NEW ERROR (TypeLoadException)
5. **Current**: Version conflict between Npgsql 10.0.1 and EF Core 8.0.0

---

## 🗄️ DATABASE STRUCTURE (RUN SQL BELOW IN pgAdmin)

### ⚠️ IMPORTANT: Run these SQL queries and paste results back

```sql
-- ========================================
-- 1. CHECK ALL ENUM DEFINITIONS
-- ========================================
SELECT 
    t.typname AS enum_name,
    e.enumlabel AS enum_value,
    e.enumsortorder AS sort_order
FROM pg_enum e
JOIN pg_type t ON e.enumtypid = t.oid
WHERE t.typname IN ('user_role', 'report_status', 'waste_type', 'image_type', 'point_transaction_type', 'team_type')
ORDER BY t.typname, e.enumsortorder;

-- ========================================
-- 2. CHECK USERS TABLE STRUCTURE
-- ========================================
SELECT 
    column_name,
    data_type,
    udt_name,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'Users'
ORDER BY ordinal_position;

-- ========================================
-- 3. CHECK ACTUAL DATA IN USERS TABLE
-- ========================================
SELECT 
    "UserId",
    "Email",
    "Role"::text AS role_text,
    "FullName",
    "Status"
FROM "Users"
ORDER BY "UserId";

-- ========================================
-- 4. CHECK DISTINCT ROLE VALUES
-- ========================================
SELECT DISTINCT "Role"::text AS unique_roles
FROM "Users";

-- ========================================
-- 5. COUNT RECORDS IN MAIN TABLES
-- ========================================
SELECT 
    'Users' AS table_name, 
    COUNT(*) AS record_count 
FROM "Users"
UNION ALL
SELECT 'Citizens', COUNT(*) FROM "Citizens"
UNION ALL
SELECT 'Collectors', COUNT(*) FROM "Collectors"
UNION ALL
SELECT 'WasteReports', COUNT(*) FROM "WasteReports";

-- ========================================
-- 6. CHECK POSTGRES VERSION
-- ========================================
SELECT version();
```

---

## ❓ QUESTIONS FOR CHATGPT

### Primary Question
**How to fix the TypeLoadException while maintaining compatibility with existing database?**

### Specific Questions

1. **Should we downgrade Npgsql from 10.0.1 to 8.0.x?**
   - If yes, which version exactly? (8.0.8, 8.0.10?)
   - Will this break anything else?

2. **Or should we upgrade the entire stack?**
   - EF Core 8.0.0 → 9.0.x
   - Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0 → 9.0.x or 10.x
   - Target .NET 9.0?

3. **Database enum values**:
   - What case should they be? PascalCase ('Admin') or lowercase ('admin')?
   - Do we need to recreate the enums?

4. **Is there a way to fix WITHOUT changing database enum values?**
   - Custom name translator?
   - Different mapping approach?

### Constraints
- ✅ Both `MapEnum<UserRole>()` and `HasPostgresEnum<UserRole>()` already in place
- ❌ Database already has data (Users, Citizens, WasteReports)
- ❌ Prefer NOT to recreate database enums if possible
- ✅ Can change C# code freely
- ✅ Can change package versions

---

## 🎯 DESIRED OUTCOME

1. ✅ GET /api/Citizen returns 200 OK with citizen data
2. ✅ No TypeLoadException or InvalidCastException
3. ✅ Compatible package versions
4. ✅ Minimal database changes (if any)

---

## 📤 NEXT STEPS

1. Run the SQL queries above in pgAdmin
2. Copy ALL results
3. Paste results in response
4. ChatGPT will provide exact fix based on actual enum values in database

