# PROJECT SETUP COMMANDS
## Step-by-Step Guide to Create Backend Structure

---

## 📋 PREREQUISITES

Before starting, ensure you have:
- ✅ .NET SDK 8.0 or 9.0 installed
- ✅ Visual Studio 2022 / Rider / VS Code
- ✅ PostgreSQL 15+ installed and running
- ✅ Git installed
- ✅ Postman / Thunder Client (for API testing)

---

## 🚀 STEP 1: CREATE SOLUTION AND PROJECTS

### 1.1 Create Solution
```bash
# Navigate to your project directory
cd /path/to/your/projects

# Create solution
dotnet new sln -n WasteCollectionPlatform

# Create solution folder
mkdir WasteCollectionPlatform
cd WasteCollectionPlatform
```

---

### 1.2 Create Projects

```bash
# Create Common Layer (Class Library)
dotnet new classlib -n WasteCollectionPlatform.Common
dotnet sln add WasteCollectionPlatform.Common/WasteCollectionPlatform.Common.csproj

# Create DataAccess Layer (Class Library)
dotnet new classlib -n WasteCollectionPlatform.DataAccess
dotnet sln add WasteCollectionPlatform.DataAccess/WasteCollectionPlatform.DataAccess.csproj

# Create Business Layer (Class Library)
dotnet new classlib -n WasteCollectionPlatform.Business
dotnet sln add WasteCollectionPlatform.Business/WasteCollectionPlatform.Business.csproj

# Create API Layer (Web API)
dotnet new webapi -n WasteCollectionPlatform.API
dotnet sln add WasteCollectionPlatform.API/WasteCollectionPlatform.API.csproj
```

---

### 1.3 Add Project References

```bash
# API depends on Business and Common
cd WasteCollectionPlatform.API
dotnet add reference ../WasteCollectionPlatform.Business/WasteCollectionPlatform.Business.csproj
dotnet add reference ../WasteCollectionPlatform.Common/WasteCollectionPlatform.Common.csproj

# Business depends on DataAccess and Common
cd ../WasteCollectionPlatform.Business
dotnet add reference ../WasteCollectionPlatform.DataAccess/WasteCollectionPlatform.DataAccess.csproj
dotnet add reference ../WasteCollectionPlatform.Common/WasteCollectionPlatform.Common.csproj

# DataAccess depends on Common
cd ../WasteCollectionPlatform.DataAccess
dotnet add reference ../WasteCollectionPlatform.Common/WasteCollectionPlatform.Common.csproj

cd ..
```

---

## 📦 STEP 2: INSTALL NUGET PACKAGES

### 2.1 Common Project (No dependencies needed)
```bash
# No packages required - this is just models
```

---

### 2.2 DataAccess Project
```bash
cd WasteCollectionPlatform.DataAccess

# Entity Framework Core with PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0

cd ..
```

---

### 2.3 Business Project
```bash
cd WasteCollectionPlatform.Business

# FluentValidation for input validation
dotnet add package FluentValidation --version 11.9.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.9.0

cd ..
```

---

### 2.4 API Project
```bash
cd WasteCollectionPlatform.API

# JWT Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

# Swagger/OpenAPI
dotnet add package Swashbuckle.AspNetCore --version 6.5.0

# SignalR (already included in ASP.NET Core, but add if needed)
# dotnet add package Microsoft.AspNetCore.SignalR --version 8.0.0

# CORS (already included)
# dotnet add package Microsoft.AspNetCore.Cors --version 8.0.0

# For file upload/image processing (optional)
dotnet add package SixLabors.ImageSharp --version 3.1.0

cd ..
```

---

## 🏗️ STEP 3: CREATE FOLDER STRUCTURE

### 3.1 Common Project Folders
```bash
cd WasteCollectionPlatform.Common

# Create folder structure
mkdir -p DTOs/Request/Auth
mkdir -p DTOs/Request/WasteReport
mkdir -p DTOs/Request/Enterprise
mkdir -p DTOs/Request/Collector
mkdir -p DTOs/Response/Auth
mkdir -p DTOs/Response/WasteReport
mkdir -p DTOs/Response/User
mkdir -p DTOs/Response/Common
mkdir Enums
mkdir Constants
mkdir Exceptions
mkdir Helpers

cd ..
```

---

### 3.2 DataAccess Project Folders
```bash
cd WasteCollectionPlatform.DataAccess

mkdir Context
mkdir Entities
mkdir -p Repositories/Interfaces
mkdir -p Repositories/Implementations
mkdir Configurations
mkdir Migrations
mkdir Seed

cd ..
```

---

### 3.3 Business Project Folders
```bash
cd WasteCollectionPlatform.Business

mkdir -p Services/Interfaces
mkdir -p Services/Implementations
mkdir StateMachines
mkdir BusinessRules
mkdir Validators

cd ..
```

---

### 3.4 API Project Folders
```bash
cd WasteCollectionPlatform.API

mkdir Controllers
mkdir Middleware
mkdir Hubs
mkdir Filters
mkdir Extensions

# Remove default files
rm Controllers/WeatherForecastController.cs 2>/dev/null || true
rm WeatherForecast.cs 2>/dev/null || true

cd ..
```

---

## 🗄️ STEP 4: CONFIGURE DATABASE

### 4.1 Create PostgreSQL Database
```sql
-- Run in PostgreSQL
CREATE DATABASE WasteCollectionDB;

-- Create user (optional)
CREATE USER wasteapp WITH PASSWORD 'YourSecurePassword123!';
GRANT ALL PRIVILEGES ON DATABASE WasteCollectionDB TO wasteapp;
```

---

### 4.2 Update appsettings.json in API Project

**File:** `WasteCollectionPlatform.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=WasteCollectionDB;Username=postgres;Password=YourPassword"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "WasteCollectionPlatform",
    "Audience": "WasteCollectionPlatformUsers",
    "ExpirationMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

---

## 🔧 STEP 5: CREATE INITIAL FILES

### 5.1 Create Enums (Common Project)

**File:** `WasteCollectionPlatform.Common/Enums/UserRole.cs`
```csharp
namespace WasteCollectionPlatform.Common.Enums;

public enum UserRole
{
    Citizen = 1,
    Enterprise = 2,
    Collector = 3,
    Admin = 4
}
```

**File:** `WasteCollectionPlatform.Common/Enums/ReportStatus.cs`
```csharp
namespace WasteCollectionPlatform.Common.Enums;

public enum ReportStatus
{
    Pending = 1,
    Accepted = 2,
    Assigned = 3,
    OnTheWay = 4,
    Collected = 5,
    Failed = 6
}
```

**File:** `WasteCollectionPlatform.Common/Enums/WasteType.cs`
```csharp
namespace WasteCollectionPlatform.Common.Enums;

public enum WasteType
{
    Plastic = 1,
    Paper = 2,
    Metal = 3,
    Organic = 4,
    Electronic = 5,
    Hazardous = 6,
    Mixed = 7
}
```

---

### 5.2 Create DbContext (DataAccess Project)

**File:** `WasteCollectionPlatform.DataAccess/Context/WasteCollectionDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Context;

public class WasteCollectionDbContext : DbContext
{
    public WasteCollectionDbContext(DbContextOptions<WasteCollectionDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Citizen> Citizens { get; set; }
    public DbSet<Enterprise> Enterprises { get; set; }
    public DbSet<Collector> Collectors { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<WasteReport> WasteReports { get; set; }
    public DbSet<ReportImage> ReportImages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PointHistory> PointHistories { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WasteCollectionDbContext).Assembly);
    }
}
```

---

### 5.3 Update Program.cs (API Project)

**File:** `WasteCollectionPlatform.API/Program.cs`
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WasteCollectionPlatform.DataAccess.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Waste Collection Platform API",
        Version = "v1",
        Description = "API for Crowdsourced Waste Collection & Home Recycling Platform"
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure PostgreSQL Database
builder.Services.AddDbContext<WasteCollectionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// TODO: Register Services (Business Layer)
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IWasteReportService, WasteReportService>();
// ... etc

// TODO: Register Repositories (Data Layer)
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// ... etc

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waste Collection Platform API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
// app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
```

---

## 🏃 STEP 6: BUILD AND RUN

### 6.1 Build Solution
```bash
# From solution root directory
dotnet build

# If successful, you should see:
# Build succeeded.
```

---

### 6.2 Create First Migration
```bash
cd WasteCollectionPlatform.API

# Create initial migration (AFTER implementing entities)
dotnet ef migrations add InitialCreate --project ../WasteCollectionPlatform.DataAccess

# Apply migration to database
dotnet ef database update --project ../WasteCollectionPlatform.DataAccess

cd ..
```

---

### 6.3 Run API
```bash
cd WasteCollectionPlatform.API

# Run with hot reload
dotnet watch run

# Or run normally
dotnet run

# API should be available at:
# https://localhost:7xxx (HTTPS)
# http://localhost:5xxx (HTTP)
# Swagger UI: https://localhost:7xxx/swagger
```

---

## 📝 STEP 7: VERIFY SETUP

### 7.1 Check Swagger
Open browser: `https://localhost:7xxx/swagger`

You should see:
- ✅ Swagger UI loads
- ✅ API documentation is visible
- ✅ JWT authentication is configured (lock icon visible)

---

### 7.2 Check Database
```sql
-- Connect to PostgreSQL
\c WasteCollectionDB

-- List tables (should be empty initially, but connection works)
\dt
```

---

## 🎯 NEXT STEPS FOR DEVELOPMENT

### Phase 1: Core Entities (Week 1)
1. ✅ Implement all Entity classes in `DataAccess/Entities/`
2. ✅ Create Entity Configurations in `DataAccess/Configurations/`
3. ✅ Run migrations
4. ✅ Create seed data (Admin account, Districts)

### Phase 2: Repository Layer (Week 2)
1. ✅ Implement `IGenericRepository` and `GenericRepository`
2. ✅ Implement specific repositories (UserRepository, WasteReportRepository, etc.)
3. ✅ Implement Unit of Work pattern

### Phase 3: Business Layer (Week 3-4)
1. ✅ Implement all Service interfaces
2. ✅ Implement business logic in Service classes
3. ✅ Implement State Machine for ReportStatus
4. ✅ Implement Business Rules validators
5. ✅ Add FluentValidation validators

### Phase 4: API Layer (Week 5-6)
1. ✅ Implement all Controllers
2. ✅ Add JWT authentication middleware
3. ✅ Add exception handling middleware
4. ✅ Implement SignalR hub
5. ✅ Test all endpoints with Swagger/Postman

### Phase 5: Testing & Deployment (Week 7)
1. ✅ Integration testing
2. ✅ Performance testing
3. ✅ Security testing
4. ✅ Deploy to cloud (optional)

---

## 🐛 COMMON ISSUES AND SOLUTIONS

### Issue 1: "Package X not found"
**Solution:**
```bash
dotnet restore
dotnet build
```

---

### Issue 2: "Database connection failed"
**Solution:**
- Check PostgreSQL is running: `sudo systemctl status postgresql`
- Verify connection string in `appsettings.json`
- Test connection: `psql -U postgres -d WasteCollectionDB`

---

### Issue 3: "Migration failed"
**Solution:**
```bash
# Drop database and recreate
dotnet ef database drop --project ../WasteCollectionPlatform.DataAccess
dotnet ef database update --project ../WasteCollectionPlatform.DataAccess
```

---

### Issue 4: "Port already in use"
**Solution:**
Update `launchSettings.json` in API project to use different ports.

---

## 📚 USEFUL COMMANDS REFERENCE

```bash
# Build solution
dotnet build

# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Run API
dotnet run --project WasteCollectionPlatform.API

# Watch mode (hot reload)
dotnet watch run --project WasteCollectionPlatform.API

# Create migration
dotnet ef migrations add <MigrationName> --project WasteCollectionPlatform.DataAccess --startup-project WasteCollectionPlatform.API

# Update database
dotnet ef database update --project WasteCollectionPlatform.DataAccess --startup-project WasteCollectionPlatform.API

# List migrations
dotnet ef migrations list --project WasteCollectionPlatform.DataAccess --startup-project WasteCollectionPlatform.API

# Remove last migration
dotnet ef migrations remove --project WasteCollectionPlatform.DataAccess --startup-project WasteCollectionPlatform.API

# Generate SQL script
dotnet ef migrations script --project WasteCollectionPlatform.DataAccess --startup-project WasteCollectionPlatform.API
```

---

## ✅ CHECKLIST

Before starting development, ensure:
- [ ] All projects created and building successfully
- [ ] All NuGet packages installed
- [ ] Database connection working
- [ ] Swagger UI accessible
- [ ] Folder structure matches specification
- [ ] Git repository initialized
- [ ] Team members have access to repository

---

**Ready to code! 🚀**
