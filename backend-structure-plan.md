# WASTE COLLECTION & RECYCLING PLATFORM - BACKEND STRUCTURE
## Architecture: 3-Layer (Presentation → Business Logic → Data Access)

### ROOT STRUCTURE
```
WasteCollectionPlatform/
├── WasteCollectionPlatform.sln                 # Solution file
├── README.md
├── .gitignore
│
├── WasteCollectionPlatform.API/               # Layer 1: Presentation (Web API)
├── WasteCollectionPlatform.Business/          # Layer 2: Business Logic
├── WasteCollectionPlatform.DataAccess/        # Layer 3: Data Access
└── WasteCollectionPlatform.Common/            # Shared: DTOs, Enums, Constants
```

---

## LAYER 1: PRESENTATION LAYER (API Project)
**WasteCollectionPlatform.API/**

### Structure:
```
WasteCollectionPlatform.API/
├── WasteCollectionPlatform.API.csproj
├── Program.cs                                 # Entry point, DI configuration
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/                               # API Endpoints
│   ├── AuthController.cs                     # Login, Register
│   ├── CitizenController.cs                  # Citizen features
│   ├── EnterpriseController.cs               # Enterprise features
│   ├── CollectorController.cs                # Collector features
│   ├── AdminController.cs                    # Admin management
│   ├── WasteReportController.cs              # Report CRUD
│   ├── NotificationController.cs             # Real-time notifications
│   ├── VoucherController.cs                  # Voucher redemption
│   └── DistrictController.cs                 # District management
│
├── Middleware/                                # Custom middleware
│   ├── ExceptionHandlingMiddleware.cs        # Global error handling
│   └── JwtAuthenticationMiddleware.cs        # JWT validation
│
├── Hubs/                                      # SignalR Hubs
│   └── NotificationHub.cs                    # Real-time notification hub
│
├── Filters/                                   # Action Filters
│   ├── AuthorizeRolesAttribute.cs            # Role-based authorization
│   └── ValidateModelAttribute.cs             # Model validation
│
└── Extensions/                                # Extension methods
    └── ServiceCollectionExtensions.cs        # DI registration helpers
```

---

## LAYER 2: BUSINESS LOGIC LAYER
**WasteCollectionPlatform.Business/**

### Structure:
```
WasteCollectionPlatform.Business/
├── WasteCollectionPlatform.Business.csproj
│
├── Services/                                  # Business logic implementation
│   ├── Interfaces/                           # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IWasteReportService.cs
│   │   ├── IUserService.cs
│   │   ├── IEnterpriseService.cs
│   │   ├── ICollectorService.cs
│   │   ├── IPointService.cs
│   │   ├── IVoucherService.cs
│   │   ├── INotificationService.cs
│   │   └── IDistrictService.cs
│   │
│   └── Implementations/                      # Service implementations
│       ├── AuthService.cs
│       ├── WasteReportService.cs
│       ├── UserService.cs
│       ├── EnterpriseService.cs
│       ├── CollectorService.cs
│       ├── PointService.cs
│       ├── VoucherService.cs
│       ├── NotificationService.cs
│       └── DistrictService.cs
│
├── StateMachines/                            # State management
│   └── ReportStatusStateMachine.cs           # Pending→Accepted→Assigned→OnTheWay→Collected→Failed
│
├── BusinessRules/                            # Business rule validators
│   ├── EnterpriseCapacityRule.cs            # R3: Capacity limits
│   ├── ReportDistributionRule.cs            # R2: Location/waste type matching
│   ├── PointCalculationRule.cs              # R4: Points calculation
│   └── ComplaintTimeframeRule.cs            # R5: 24-hour complaint window
│
└── Validators/                               # FluentValidation validators
    ├── WasteReportValidator.cs
    ├── UserRegistrationValidator.cs
    └── EnterpriseProfileValidator.cs
```

---

## LAYER 3: DATA ACCESS LAYER
**WasteCollectionPlatform.DataAccess/**

### Structure:
```
WasteCollectionPlatform.DataAccess/
├── WasteCollectionPlatform.DataAccess.csproj
│
├── Context/
│   └── WasteCollectionDbContext.cs           # EF Core DbContext
│
├── Entities/                                  # Database models (tables)
│   ├── User.cs                               # Base user table
│   ├── Citizen.cs
│   ├── Enterprise.cs
│   ├── Collector.cs
│   ├── District.cs
│   ├── WasteReport.cs
│   ├── ReportImage.cs
│   ├── Notification.cs
│   ├── PointHistory.cs
│   └── Voucher.cs
│
├── Repositories/                              # Data access abstraction
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs             # Base CRUD operations
│   │   ├── IUserRepository.cs
│   │   ├── IWasteReportRepository.cs
│   │   ├── IEnterpriseRepository.cs
│   │   ├── ICollectorRepository.cs
│   │   ├── IDistrictRepository.cs
│   │   ├── IPointHistoryRepository.cs
│   │   ├── IVoucherRepository.cs
│   │   └── IUnitOfWork.cs                    # Transaction management
│   │
│   └── Implementations/
│       ├── GenericRepository.cs
│       ├── UserRepository.cs
│       ├── WasteReportRepository.cs
│       ├── EnterpriseRepository.cs
│       ├── CollectorRepository.cs
│       ├── DistrictRepository.cs
│       ├── PointHistoryRepository.cs
│       ├── VoucherRepository.cs
│       └── UnitOfWork.cs
│
├── Configurations/                            # EF Core entity configurations
│   ├── UserConfiguration.cs
│   ├── WasteReportConfiguration.cs
│   ├── EnterpriseConfiguration.cs
│   └── ... (one per entity)
│
├── Migrations/                                # EF Core migrations (auto-generated)
│
└── Seed/                                      # Initial data seeding
    └── DataSeeder.cs
```

---

## SHARED LAYER: COMMON
**WasteCollectionPlatform.Common/**

### Structure:
```
WasteCollectionPlatform.Common/
├── WasteCollectionPlatform.Common.csproj
│
├── DTOs/                                      # Data Transfer Objects
│   ├── Request/                              # API request models
│   │   ├── Auth/
│   │   │   ├── LoginRequestDto.cs
│   │   │   └── RegisterRequestDto.cs
│   │   ├── WasteReport/
│   │   │   ├── CreateReportRequestDto.cs
│   │   │   └── UpdateReportStatusDto.cs
│   │   ├── Enterprise/
│   │   │   ├── AcceptReportDto.cs
│   │   │   └── AssignCollectorDto.cs
│   │   └── Collector/
│   │       ├── UpdateCollectionStatusDto.cs
│   │       └── MarkReportInvalidDto.cs
│   │
│   └── Response/                             # API response models
│       ├── Auth/
│       │   └── AuthResponseDto.cs
│       ├── WasteReport/
│       │   ├── WasteReportDto.cs
│       │   └── ReportHistoryDto.cs
│       ├── User/
│       │   └── UserProfileDto.cs
│       └── Common/
│           ├── ApiResponse.cs               # Standardized API response
│           └── PaginatedResponse.cs
│
├── Enums/                                     # Enumerations
│   ├── UserRole.cs                           # Citizen, Enterprise, Collector, Admin
│   ├── ReportStatus.cs                       # Pending, Accepted, Assigned, OnTheWay, Collected, Failed
│   ├── WasteType.cs                          # Plastic, Paper, Metal, Organic, etc.
│   └── NotificationType.cs
│
├── Constants/                                 # Application constants
│   ├── ErrorMessages.cs
│   ├── SuccessMessages.cs
│   └── AppSettings.cs
│
├── Exceptions/                                # Custom exceptions
│   ├── BusinessRuleException.cs
│   ├── NotFoundException.cs
│   └── UnauthorizedException.cs
│
└── Helpers/                                   # Utility classes
    ├── JwtHelper.cs                          # JWT token generation
    ├── PasswordHasher.cs                     # Password hashing
    ├── GPSHelper.cs                          # GPS distance calculation
    └── ImageHelper.cs                        # Image processing utilities
```

---

## KEY FILES CONTENT GUIDELINES

### 1. **Program.cs** (API Entry Point)
**Configuration checklist:**
- Database connection (PostgreSQL)
- JWT authentication setup
- CORS policy
- Swagger/OpenAPI
- SignalR hub mapping
- Dependency Injection for all layers
- Exception handling middleware
- Entity Framework migrations

### 2. **WasteCollectionDbContext.cs** (Database Context)
**Must include:**
- DbSet for all entities
- Configure relationships (FK, navigation properties)
- Apply configurations from Configurations folder
- Override OnModelCreating for fluent API

### 3. **ReportStatusStateMachine.cs** (State Management)
**State transitions:**
```
Pending → Accepted (by Enterprise)
Accepted → Assigned (assign collector)
Assigned → OnTheWay (collector en route)
OnTheWay → Collected (successful) OR Failed (invalid report)
```

### 4. **IUnitOfWork.cs** (Transaction Pattern)
**Methods:**
- SaveChangesAsync()
- BeginTransactionAsync()
- CommitAsync()
- RollbackAsync()

---

## PROJECT DEPENDENCIES

### **API Project** depends on:
- Business Layer
- Common Layer
- NuGet: Microsoft.AspNetCore.Authentication.JwtBearer
- NuGet: Swashbuckle.AspNetCore (Swagger)
- NuGet: SignalR

### **Business Layer** depends on:
- DataAccess Layer
- Common Layer
- NuGet: FluentValidation

### **DataAccess Layer** depends on:
- Common Layer (for Enums)
- NuGet: Npgsql.EntityFrameworkCore.PostgreSQL
- NuGet: Microsoft.EntityFrameworkCore.Tools

### **Common Layer** has NO dependencies (pure models)

---

## FOLDER NAMING CONVENTIONS
- ✅ PascalCase for folders: `Controllers/`, `Services/`
- ✅ Interface prefix: `IAuthService.cs`
- ✅ Suffix for types: `AuthController.cs`, `LoginRequestDto.cs`
- ✅ Plural for collections: `Entities/`, `DTOs/`

---

## NEXT STEPS FOR YOUR TEAM
1. **Create Solution & Projects** (use `dotnet new` commands)
2. **Add Project References** (API → Business → DataAccess)
3. **Install NuGet Packages** (see dependencies above)
4. **Create DbContext & Entities** (match ERD from scope)
5. **Implement Repositories** (Generic + Specific)
6. **Implement Services** (Business logic)
7. **Create Controllers** (API endpoints)
8. **Add Migrations** (`Add-Migration InitialCreate`)
9. **Seed Data** (Admin account, sample districts)
10. **Test with Swagger**

