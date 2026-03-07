# 🗑️ Waste Collection & Recycling Platform - Backend

> **3-Layer Architecture** | **ASP.NET Core Web API** | **PostgreSQL** | **SignalR**

A comprehensive backend system for crowdsourced waste collection and home recycling platform, connecting Citizens, Recycling Enterprises, and Collectors to optimize source-separated waste management in urban areas.

---

## 📋 Project Overview

### Objectives
- Build a digital platform for efficient waste collection coordination
- Implement source-separated waste collection with GPS tracking
- Provide a points-based rewards system to encourage citizen participation
- Ensure transparency in urban waste management operations

### Key Features
✅ Waste reporting with photos and GPS coordinates  
✅ Smart coordination between enterprises and collectors  
✅ Reward points system for citizens  
✅ Real-time notifications via SignalR  
✅ District-based organization  
✅ Voucher redemption system  
✅ Admin dashboard for system monitoring  
✅ JWT-based authentication and authorization  

---

## 🏗️ Architecture

### 3-Layer Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│              (WasteCollectionPlatform.API)                   │
│  Controllers | Middleware | SignalR Hubs | Filters          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│             (WasteCollectionPlatform.Business)               │
│  Services | State Machines | Business Rules | Validators    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                         │
│            (WasteCollectionPlatform.DataAccess)              │
│   DbContext | Entities | Repositories | Configurations      │
└─────────────────────────────────────────────────────────────┘
                            ↓
                  ┌──────────────────┐
                  │   PostgreSQL DB   │
                  └──────────────────┘
```

### Project Structure

```
WasteCollectionPlatform/
├── WasteCollectionPlatform.API/           # Presentation Layer
│   ├── Controllers/                       # API endpoints
│   ├── Middleware/                        # Custom middleware
│   ├── Hubs/                             # SignalR hubs
│   └── Program.cs                        # App configuration
│
├── WasteCollectionPlatform.Business/     # Business Logic Layer
│   ├── Services/                         # Business logic
│   ├── StateMachines/                    # Status flow management
│   ├── BusinessRules/                    # Business rule validators
│   └── Validators/                       # Input validation
│
├── WasteCollectionPlatform.DataAccess/   # Data Access Layer
│   ├── Context/                          # EF Core DbContext
│   ├── Entities/                         # Database models
│   ├── Repositories/                     # Data access abstraction
│   └── Configurations/                   # Entity configurations
│
└── WasteCollectionPlatform.Common/       # Shared Layer
    ├── DTOs/                             # Data transfer objects
    ├── Enums/                            # Enumerations
    ├── Constants/                        # Application constants
    └── Helpers/                          # Utility classes
```

---

## 🛠️ Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core Web API | .NET 8/9 |
| **Language** | C# | Latest |
| **Database** | PostgreSQL | 15+ |
| **ORM** | Entity Framework Core | 8.0 |
| **Authentication** | JWT (JSON Web Token) | - |
| **Real-time** | SignalR | - |
| **Validation** | FluentValidation | 11.9.0 |
| **API Documentation** | Swagger/OpenAPI | - |
| **Architecture** | 3-Layer (Clean Architecture) | - |

---

## 📊 Database Schema

### Core Entities

1. **User** (Base table - TPT inheritance)
   - Citizen, Enterprise, Collector, Admin

2. **District** (Geographic organization)

3. **WasteReport** (Main transaction)
   - Status flow: Pending → Accepted → Assigned → OnTheWay → Collected/Failed

4. **ReportImage** (Photo evidence)

5. **Notification** (Real-time updates)

6. **PointHistory** (Points ledger)

7. **Voucher** (Reward system)

**See:** `ENTITY_RELATIONSHIPS.md` for detailed ERD

---

## 🔌 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Citizen
- `POST /api/citizen/reports` - Create waste report
- `GET /api/citizen/reports` - Get report history
- `GET /api/citizen/points` - View points balance

### Enterprise
- `GET /api/enterprise/reports/pending` - View pending reports
- `POST /api/enterprise/reports/{id}/accept` - Accept report
- `POST /api/enterprise/reports/{id}/assign` - Assign collector

### Collector
- `GET /api/collector/tasks/assigned` - View assigned tasks
- `PUT /api/collector/tasks/{id}/status` - Update task status
- `POST /api/collector/tasks/{id}/complete` - Complete collection

### Admin
- `GET /api/admin/users` - Manage users
- `PUT /api/admin/users/{id}/approve` - Approve enterprise
- `GET /api/admin/dashboard/statistics` - View system stats

**See:** `API_ENDPOINTS.md` for complete API specification

---

## 🚀 Getting Started

### Prerequisites
- .NET SDK 8.0 or 9.0
- PostgreSQL 15+
- Visual Studio 2022 / Rider / VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WasteCollectionPlatform
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database**
   - Update `appsettings.json` with your PostgreSQL connection string
   - Create database: `CREATE DATABASE WasteCollectionDB;`

4. **Run migrations**
   ```bash
   cd WasteCollectionPlatform.API
   dotnet ef database update --project ../WasteCollectionPlatform.DataAccess
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   - Navigate to: `https://localhost:7xxx/swagger`

**See:** `SETUP_COMMANDS.md` for detailed setup instructions

---

## 📁 Documentation

| Document | Description |
|----------|-------------|
| `ENTITY_RELATIONSHIPS.md` | Complete ERD and entity documentation |
| `API_ENDPOINTS.md` | Full API specification with examples |
| `SETUP_COMMANDS.md` | Step-by-step setup guide |
| `backend-structure-plan.md` | Detailed architecture documentation |

---

## 🎯 Business Rules

| Rule | Description | Implementation |
|------|-------------|----------------|
| **R1** | Enterprise activation requires Admin approval | Status field validation |
| **R2** | Reports filtered by location and waste type | Query filtering logic |
| **R3** | Daily capacity limit enforcement | Volume tracking per enterprise |
| **R4** | Points calculation formula | Volume × Waste Type × Quality Index |
| **R5** | 24-hour complaint window | Timestamp validation |

---

## 🔒 Security Features

- ✅ JWT-based authentication
- ✅ Role-based authorization (Citizen, Enterprise, Collector, Admin)
- ✅ Password hashing with BCrypt
- ✅ CORS configuration
- ✅ Input validation with FluentValidation
- ✅ SQL injection prevention (EF Core parameterization)
- ✅ HTTPS enforcement

---

## 🧪 Testing

### Unit Tests (Coming soon)
```bash
dotnet test
```

### Integration Tests (Coming soon)
- API endpoint testing
- Database integration testing
- SignalR hub testing

### API Testing
Use Swagger UI or Postman collection for manual testing

---

## 📈 Development Workflow

### Phase 1: Core Entities (Week 1)
- [ ] Implement all Entity classes
- [ ] Create Entity Configurations
- [ ] Run initial migrations
- [ ] Create seed data

### Phase 2: Repository Layer (Week 2)
- [ ] Implement Generic Repository
- [ ] Implement specific repositories
- [ ] Implement Unit of Work

### Phase 3: Business Layer (Week 3-4)
- [ ] Implement all Service interfaces
- [ ] Add business logic
- [ ] Create State Machine
- [ ] Add validators

### Phase 4: API Layer (Week 5-6)
- [ ] Implement Controllers
- [ ] Add Middleware
- [ ] Setup SignalR
- [ ] Configure JWT

### Phase 5: Testing & Deployment (Week 7)
- [ ] Integration testing
- [ ] Performance testing
- [ ] Security audit
- [ ] Deployment

---

## 🤝 Team Roles

| Member | Responsibility |
|--------|---------------|
| **Nguyễn Minh Đức** | Project Lead |
| **Nguyễn Trường Thọ** | Backend Developer |
| **Nguyễn Bá Chinh** | Backend Developer |
| **Trần Văn Nhật** | Backend Developer |
| **Huỳnh Đăng Trường** | Backend Developer |

---

## 📝 Coding Standards

### Naming Conventions
- **Classes:** PascalCase (e.g., `WasteReportService`)
- **Methods:** PascalCase (e.g., `CreateReport()`)
- **Variables:** camelCase (e.g., `reportId`)
- **Interfaces:** I-prefix (e.g., `IWasteReportService`)
- **Private fields:** _camelCase (e.g., `_repository`)

### File Organization
- One class per file
- File name matches class name
- Group related files in folders

### Comments
- XML documentation for public APIs
- Inline comments for complex logic
- No commented-out code in commits

---

## 🐛 Troubleshooting

### Common Issues

**Issue:** Database connection failed  
**Solution:** Check PostgreSQL is running and connection string is correct

**Issue:** Migration failed  
**Solution:** Drop database and recreate: `dotnet ef database drop`

**Issue:** Port already in use  
**Solution:** Change port in `launchSettings.json`

**See:** `SETUP_COMMANDS.md` for more solutions

---

## 📄 License

This project is developed for educational purposes as part of SWP391 course.

---

## 📞 Contact

For questions or issues, please contact the team lead or create an issue in the repository.

---

**Last Updated:** February 2026  
**Version:** 1.0.0  
**Status:** In Development 🚧
t e s t  
 