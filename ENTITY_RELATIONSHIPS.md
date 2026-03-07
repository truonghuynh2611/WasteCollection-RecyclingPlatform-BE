# ENTITY RELATIONSHIPS GUIDE
## Based on ERD from Scope Document

---

## 📋 TABLE OVERVIEW

### Core Tables:
1. **User** (Base table - inheritance)
2. **Citizen** (extends User)
3. **Enterprise** (extends User) 
4. **Collector** (extends User)
5. **District** (Geographic division)
6. **WasteReport** (Main transaction)
7. **ReportImage** (Report photos)
8. **Notification** (Real-time updates)
9. **PointHistory** (Points transactions)
10. **Voucher** (Reward redemption)

---

## 🔗 ENTITY RELATIONSHIPS

### 1️⃣ USER (Base Table - TPT Pattern)
```csharp
// User.cs - Base entity
public class User
{
    public Guid UserId { get; set; }  // PK
    public string FullName { get; set; }
    public string Email { get; set; }  // Unique
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public UserRole Role { get; set; }  // Enum: Citizen, Enterprise, Collector, Admin
    public UserStatus Status { get; set; }  // Enum: Active, Inactive, Pending
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Notification> Notifications { get; set; }
}
```

**Relationships:**
- 1 User → N Notifications (UserId FK)

---

### 2️⃣ CITIZEN (Extends User)
```csharp
public class Citizen
{
    public Guid CitizenId { get; set; }  // PK, also FK to User
    public Guid UserId { get; set; }  // FK to User
    public int TotalPoints { get; set; }  // Accumulated points
    
    // Navigation properties
    public User User { get; set; }
    public ICollection<WasteReport> WasteReports { get; set; }
    public ICollection<PointHistory> PointHistories { get; set; }
}
```

**Relationships:**
- 1 Citizen → 1 User (UserId FK)
- 1 Citizen → N WasteReports (CitizenId FK)
- 1 Citizen → N PointHistories (CitizenId FK)

---

### 3️⃣ ENTERPRISE (Extends User)
```csharp
public class Enterprise
{
    public Guid EnterpriseId { get; set; }  // PK, also FK to User
    public Guid UserId { get; set; }  // FK to User
    public Guid DistrictId { get; set; }  // FK to District (service area)
    public string ServiceArea { get; set; }
    public string WasteTypesAccepted { get; set; }  // JSON or comma-separated
    public int CapacityLimit { get; set; }  // Daily limit (R3)
    public int CurrentDailyVolume { get; set; }  // Reset daily
    
    // Navigation properties
    public User User { get; set; }
    public District District { get; set; }
    public ICollection<WasteReport> AcceptedReports { get; set; }
}
```

**Relationships:**
- 1 Enterprise → 1 User (UserId FK)
- 1 Enterprise → 1 District (DistrictId FK)
- 1 Enterprise → N WasteReports (via CollectorId assignment)

**Business Rules Applied:**
- R1: Status must be approved by Admin
- R2: Only see reports matching ServiceArea + WasteTypesAccepted
- R3: Cannot accept if CurrentDailyVolume >= CapacityLimit

---

### 4️⃣ COLLECTOR (Extends User)
```csharp
public class Collector
{
    public Guid CollectorId { get; set; }  // PK, also FK to User
    public Guid UserId { get; set; }  // FK to User
    public Guid DistrictId { get; set; }  // FK to District (working area)
    public CollectorStatus Status { get; set; }  // Available, Busy
    public int CurrentTaskCount { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public District District { get; set; }
    public ICollection<WasteReport> AssignedReports { get; set; }
}
```

**Relationships:**
- 1 Collector → 1 User (UserId FK)
- 1 Collector → 1 District (DistrictId FK)
- 1 Collector → N WasteReports (CollectorId FK)

---

### 5️⃣ DISTRICT (Geographic Division)
```csharp
public class District
{
    public Guid DistrictId { get; set; }  // PK
    public string DistrictName { get; set; }  // e.g., "District 1", "Binh Thanh"
    
    // Navigation properties
    public ICollection<Enterprise> Enterprises { get; set; }
    public ICollection<Collector> Collectors { get; set; }
    public ICollection<WasteReport> Reports { get; set; }
}
```

**Relationships:**
- 1 District → N Enterprises
- 1 District → N Collectors
- 1 District → N WasteReports

---

### 6️⃣ WASTEREPORT (Core Transaction)
```csharp
public class WasteReport
{
    public Guid ReportId { get; set; }  // PK
    public Guid CitizenId { get; set; }  // FK to Citizen
    public Guid? CollectorId { get; set; }  // FK to Collector (nullable until assigned)
    public Guid DistrictId { get; set; }  // FK to District (auto-determined by GPS)
    
    public string Description { get; set; }
    public string WasteType { get; set; }  // Can be updated after assignment
    public decimal Latitude { get; set; }  // GPS coordinates
    public decimal Longitude { get; set; }
    
    public ReportStatus Status { get; set; }  // State machine
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public Citizen Citizen { get; set; }
    public Collector Collector { get; set; }
    public District District { get; set; }
    public ICollection<ReportImage> Images { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<PointHistory> PointHistories { get; set; }
}
```

**State Machine (ReportStatus Enum):**
```
Pending → Accepted → Assigned → OnTheWay → Collected
                                         ↘ Failed
```

**Relationships:**
- N WasteReports → 1 Citizen
- N WasteReports → 1 Collector (optional)
- N WasteReports → 1 District
- 1 WasteReport → N ReportImages
- 1 WasteReport → N Notifications
- 1 WasteReport → N PointHistories

---

### 7️⃣ REPORTIMAGE (Photo Evidence)
```csharp
public class ReportImage
{
    public Guid ImageId { get; set; }  // PK
    public Guid ReportId { get; set; }  // FK to WasteReport
    public string ImageUrl { get; set; }  // Storage path
    public ImageType ImageType { get; set; }  // Enum: CitizenUpload, CollectorConfirmation
    public DateTime UploadedAt { get; set; }
    
    // Navigation properties
    public WasteReport Report { get; set; }
}
```

**Relationships:**
- N ReportImages → 1 WasteReport

---

### 8️⃣ NOTIFICATION (Real-time Updates)
```csharp
public class Notification
{
    public Guid NotificationId { get; set; }  // PK
    public Guid? ReportId { get; set; }  // FK to WasteReport (nullable for system notifications)
    public Guid UserId { get; set; }  // FK to User (recipient)
    public string Message { get; set; }
    public NotificationType Type { get; set; }  // Enum: StatusUpdate, Assignment, PointsEarned
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public WasteReport Report { get; set; }
    public User User { get; set; }
}
```

**Relationships:**
- N Notifications → 1 WasteReport (optional)
- N Notifications → 1 User

---

### 9️⃣ POINTHISTORY (Points Ledger)
```csharp
public class PointHistory
{
    public Guid PointLogId { get; set; }  // PK
    public Guid CitizenId { get; set; }  // FK to Citizen
    public Guid? ReportId { get; set; }  // FK to WasteReport (for earned points)
    public Guid? VoucherId { get; set; }  // FK to Voucher (for redeemed points)
    
    public int PointAmount { get; set; }  // Positive for earn, negative for redeem/deduct
    public string Reason { get; set; }  // e.g., "Report Collected", "Invalid Report", "Voucher Redeemed"
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Citizen Citizen { get; set; }
    public WasteReport Report { get; set; }
    public Voucher Voucher { get; set; }
}
```

**Business Rule R4 (Points Calculation):**
```
Points = (Volume / WasteType) × (Photo Quality / Sorting Index)
```

**Relationships:**
- N PointHistories → 1 Citizen
- N PointHistories → 1 WasteReport (optional)
- N PointHistories → 1 Voucher (optional)

---

### 🔟 VOUCHER (Rewards)
```csharp
public class Voucher
{
    public Guid VoucherId { get; set; }  // PK
    public string VoucherName { get; set; }  // e.g., "10% Off Groceries"
    public int PointsRequired { get; set; }  // Cost in points
    public int StockQuantity { get; set; }  // Available inventory
    public DateTime? ExpiryDate { get; set; }
    
    // Navigation properties
    public ICollection<PointHistory> PointHistories { get; set; }
}
```

**Relationships:**
- 1 Voucher → N PointHistories (redemption records)

---

## 🎯 KEY FOREIGN KEY CONSTRAINTS

### Cascade Delete Rules:
```
User → Citizen: CASCADE (delete user = delete citizen)
User → Enterprise: CASCADE
User → Collector: CASCADE
Citizen → WasteReport: NO ACTION (keep reports for audit)
WasteReport → ReportImage: CASCADE (delete report = delete images)
WasteReport → Notification: CASCADE
District → WasteReport: RESTRICT (cannot delete district with reports)
```

---

## 📊 INDEXES (Performance Optimization)

### Recommended Indexes:
```sql
-- User lookups
CREATE INDEX IX_User_Email ON User(Email);
CREATE INDEX IX_User_Role ON User(Role);

-- Report queries
CREATE INDEX IX_WasteReport_Status ON WasteReport(Status);
CREATE INDEX IX_WasteReport_CitizenId ON WasteReport(CitizenId);
CREATE INDEX IX_WasteReport_CollectorId ON WasteReport(CollectorId);
CREATE INDEX IX_WasteReport_DistrictId ON WasteReport(DistrictId);
CREATE INDEX IX_WasteReport_CreatedAt ON WasteReport(CreatedAt);

-- Geographic queries
CREATE INDEX IX_WasteReport_Location ON WasteReport(Latitude, Longitude);

-- Points tracking
CREATE INDEX IX_PointHistory_CitizenId ON PointHistory(CitizenId);
CREATE INDEX IX_PointHistory_CreatedAt ON PointHistory(CreatedAt);
```

---

## 🔐 DATA VALIDATION RULES

### User Entity:
- Email: Unique, valid format
- Phone: Valid format (Vietnamese +84)
- Role: Must be valid enum value

### WasteReport Entity:
- Latitude: -90 to 90
- Longitude: -180 to 180
- Status: Follow state machine rules
- Description: Required, min 10 characters

### Enterprise Entity:
- CapacityLimit: > 0
- CurrentDailyVolume: >= 0, <= CapacityLimit

### Voucher Entity:
- PointsRequired: > 0
- StockQuantity: >= 0

---

## 📝 NOTES FOR IMPLEMENTATION

1. **Use TPT (Table-Per-Type) for User Inheritance:**
   - User table stores common fields
   - Citizen/Enterprise/Collector tables store specific fields
   - Requires proper EF Core configuration

2. **Implement Soft Delete (Optional):**
   - Add `IsDeleted` and `DeletedAt` fields
   - Filter queries to exclude deleted records

3. **Audit Trail:**
   - All entities should have `CreatedAt`, `UpdatedAt`
   - Consider adding `CreatedBy`, `UpdatedBy` for admin actions

4. **Concurrency Control:**
   - Add `RowVersion` or `Timestamp` for optimistic concurrency
   - Important for Enterprise capacity checks

5. **JSON Columns (PostgreSQL):**
   - Store `WasteTypesAccepted` as JSONB for flexible querying
   - Store report metadata as JSON if needed

---

## 🎯 BUSINESS RULES MAPPING

| Rule | Affected Entities | Implementation |
|------|------------------|----------------|
| R1: Enterprise Activation | Enterprise, User | Status field + Admin approval |
| R2: Report Distribution | WasteReport, Enterprise, District | Service area matching query |
| R3: Capacity Limit | Enterprise | CurrentDailyVolume check before accept |
| R4: Points Calculation | PointHistory, WasteReport | Business logic in PointService |
| R5: Complaint Timeframe | WasteReport | CreatedAt + 24 hours validation |

---

**Next Step:** Implement these entities in `DataAccess/Entities/` following this guide.
