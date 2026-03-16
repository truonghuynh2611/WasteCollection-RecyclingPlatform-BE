using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using WasteCollectionPlatform.API.Middleware;
using WasteCollectionPlatform.Business.Services.Implementations;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Business.Validators;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Helpers;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Repositories.Implementations;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Waste Collection Platform API",
        Version = "v1",
        Description = "API for managing waste collection and recycling platform"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure Database - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Tạo DataSource và map enum với tên translator tùy chỉnh để chuyển lowercase DB <-> PascalCase C#
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString!);
// DB stores lowercase ('admin','citizen'...) while C# uses PascalCase ('Admin','Citizen'...)
// NpgsqlSnakeCaseNameTranslator handles this via toLower mapping
dataSourceBuilder.MapEnum<UserRole>("user_role", nameTranslator: new Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator());

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<WasteManagementContext>(options =>
    options.UseNpgsql(dataSource));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register Helpers
builder.Services.AddScoped<JwtHelper>();

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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
// Add other services here as they are implemented

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

var app = builder.Build();

//Apply RefreshToken migration manually (one-time setup)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS refreshtoken (
                refreshtokenid SERIAL PRIMARY KEY,
                userid INTEGER NOT NULL,
                token VARCHAR(500) NOT NULL UNIQUE,
                expiresat TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                createdat TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                isrevoked BOOLEAN DEFAULT FALSE,
                revokedat TIMESTAMP WITHOUT TIME ZONE,
                CONSTRAINT fk_refreshtoken_user FOREIGN KEY (userid) REFERENCES ""User""(userid) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS idx_refreshtoken_token ON refreshtoken(token);
            CREATE INDEX IF NOT EXISTS idx_refreshtoken_userid ON refreshtoken(userid);
        ");
        Console.WriteLine("✅ RefreshToken table migration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  RefreshToken table already exists or migration failed: {ex.Message}");
    }
    
    // Apply Email Verification migration
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""User"" 
            ADD COLUMN IF NOT EXISTS emailverified BOOLEAN DEFAULT FALSE,
            ADD COLUMN IF NOT EXISTS verificationtoken VARCHAR(500),
            ADD COLUMN IF NOT EXISTS verificationtokenexpiry TIMESTAMP WITHOUT TIME ZONE,
            ADD COLUMN IF NOT EXISTS resetpasswordtoken VARCHAR(500),
            ADD COLUMN IF NOT EXISTS resettokenexpiry TIMESTAMP WITHOUT TIME ZONE;

            -- Ensure manager role exists in enum
            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM pg_type t JOIN pg_enum e ON t.oid = e.enumtypid WHERE t.typname = 'user_role' AND e.enumlabel = 'manager') THEN
                    ALTER TYPE user_role ADD VALUE 'manager';
                END IF;
            END $$;
            
            CREATE INDEX IF NOT EXISTS idx_users_verificationtoken ON ""User""(verificationtoken) WHERE verificationtoken IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_users_resetpasswordtoken ON ""User""(resetpasswordtoken) WHERE resetpasswordtoken IS NOT NULL;
        ");
        Console.WriteLine("✅ Email verification and password reset fields migration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Email verification fields already exist or migration failed: {ex.Message}");
    }
    
    // Apply Enterprise table migration
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS enterprise (
                enterpriseid SERIAL PRIMARY KEY,
                userid INTEGER NOT NULL UNIQUE,
                districtid INTEGER,
                wastetypes VARCHAR(255),
                dailycapacity INTEGER,
                currentload INTEGER DEFAULT 0,
                status BOOLEAN DEFAULT TRUE,
                CONSTRAINT fk_enterprise_user FOREIGN KEY (userid) REFERENCES ""User""(userid) ON DELETE CASCADE,
                CONSTRAINT fk_enterprise_district FOREIGN KEY (districtid) REFERENCES district(districtid) ON DELETE SET NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS enterprise_userid_key ON enterprise(userid);
        ");
        Console.WriteLine("✅ Enterprise table migration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Enterprise table already exists or migration failed: {ex.Message}");
    }
    
    // Seed District data
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO district (districtname) 
            SELECT * FROM (VALUES
                ('Quận 1'), ('Quận 2'), ('Quận 3'), ('Quận 4'),
                ('Quận 5'), ('Quận 6'), ('Quận 7'), ('Quận 8'),
                ('Quận 9'), ('Quận 10'), ('Quận 11'), ('Quận 12'),
                ('Bình Thạnh'), ('Tân Bình'), ('Tân Phú'),
                ('Phú Nhuận'), ('Gò Vấp'), ('Bình Tân'),
                ('Thủ Đức'), ('Củ Chi'), ('Hóc Môn'),
                ('Bình Chánh'), ('Nhà Bè'), ('Cần Giờ')
            ) AS v(districtname)
            WHERE NOT EXISTS (
                SELECT 1 FROM district WHERE districtname = v.districtname
            );
        ");
        Console.WriteLine("✅ District data seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  District seed failed (might already exist): {ex.Message}");
    }
    
    // Seed Area data (Wards) for key districts
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO area (districtid, name) 
            SELECT d.districtid, v.name
            FROM district d, (VALUES
                ('Quận 1', 'Phường Bến Nghé'), ('Quận 1', 'Phường Bến Thành'), ('Quận 1', 'Phường Cầu Kho'), 
                ('Quận 1', 'Phường Cầu Ông Lãnh'), ('Quận 1', 'Phường Cô Giang'), ('Quận 1', 'Phường Đa Kao'),
                ('Quận 1', 'Phường Nguyễn Cư Trinh'), ('Quận 1', 'Phường Nguyễn Thái Bình'), ('Quận 1', 'Phường Phạm Ngũ Lão'),
                ('Quận 1', 'Phường Tân Định'),
                
                ('Quận 3', 'Võ Thị Sáu'), ('Quận 3', 'Phường 1'), ('Quận 3', 'Phường 2'), ('Quận 3', 'Phường 3'),
                ('Quận 3', 'Phường 4'), ('Quận 3', 'Phường 5'), ('Quận 3', 'Phường 9'), ('Quận 3', 'Phường 10'),
                
                ('Quận 5', 'Phường 1'), ('Quận 5', 'Phường 2'), ('Quận 5', 'Phường 3'), ('Quận 5', 'Phường 4'),
                ('Quận 5', 'Phường 5'), ('Quận 5', 'Phường 6'), ('Quận 5', 'Phường 7'), ('Quận 5', 'Phường 8'),
                
                ('Quận 10', 'Phường 1'), ('Quận 10', 'Phường 2'), ('Quận 10', 'Phường 4'), ('Quận 10', 'Phường 5'),
                ('Quận 10', 'Phường 6'), ('Quận 10', 'Phường 7'), ('Quận 10', 'Phường 8'), ('Quận 10', 'Phường 9'),
                
                ('Tân Bình', 'Phường 1'), ('Tân Bình', 'Phường 2'), ('Tân Bình', 'Phường 3'), ('Tân Bình', 'Phường 4'),
                ('Tân Bình', 'Phường 5'), ('Tân Bình', 'Phường 6'), ('Tân Bình', 'Phường 7'), ('Tân Bình', 'Phường 8'),
                
                ('Bình Thạnh', 'Phường 1'), ('Bình Thạnh', 'Phường 2'), ('Bình Thạnh', 'Phường 3'), ('Bình Thạnh', 'Phường 5'),
                ('Bình Thạnh', 'Phường 6'), ('Bình Thạnh', 'Phường 7'), ('Bình Thạnh', 'Phường 11'), ('Bình Thạnh', 'Phường 12')
            ) AS v(districtname, name)
            WHERE d.districtname = v.districtname
            AND NOT EXISTS (
                SELECT 1 FROM area WHERE area.districtid = d.districtid AND area.name = v.name
            );
        ");
        Console.WriteLine("✅ Area data seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Area seed failed (might already exist): {ex.Message}");
    }
    
    // Seed Team data
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO team (areaid, name) 
            SELECT a.areaid, v.name
            FROM area a
            INNER JOIN district d ON a.districtid = d.districtid, (VALUES
                ('Khu vực 1A', 'Team Alpha'),
                ('Khu vực 1A', 'Team Beta'),
                ('Khu vực 1B', 'Team Gamma'),
                ('Khu vực 2A', 'Team Delta'),
                ('Khu vực 2A', 'Team Epsilon'),
                ('Khu vực 2B', 'Team Zeta'),
                ('Khu vực 3A', 'Team Eta'),
                ('Khu vực 4A', 'Team Theta'),
                ('Khu vực 7A', 'Team North'),
                ('Khu vực 7B', 'Team South'),
                ('Khu vực 9A', 'Team East'),
                ('Khu vực 12A', 'Team West')
            ) AS v(areaname, name)
            WHERE a.name = v.areaname
            AND NOT EXISTS (
                SELECT 1 FROM team WHERE team.areaid = a.areaid AND team.name = v.name
            );
        ");
        Console.WriteLine("✅ Team data seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Team seed failed (might already exist): {ex.Message}");
    }

    // Seed Sample Users (Admin, Citizen, Collector)
    try
    {
        var userCount = await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"User\" LIMIT 1");
        // ExecuteSqlRawAsync returns number of affected rows, but for SELECT it's not reliable.
        var hasUsers = await context.Users.AnyAsync();
        
        if (!hasUsers)
        {
            var defaultPassword = "password123";
            var hashedPassword = WasteCollectionPlatform.Common.Helpers.PasswordHasher.HashPassword(defaultPassword);

            // Insert Admin
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""User"" (fullname, email, phone, password, role, status, emailverified) 
                  SELECT 'System Admin', 'admin@example.com', '0900000001', {0}, 'admin', true, true
                  WHERE NOT EXISTS (SELECT 1 FROM ""User"" WHERE email = 'admin@example.com')", 
                hashedPassword);

            // Insert Citizen
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""User"" (fullname, email, phone, password, role, status, emailverified)
                  SELECT 'Citizen', 'citizen@example.com', '0900000002', {0}, 'citizen', true, true
                  WHERE NOT EXISTS (SELECT 1 FROM ""User"" WHERE email = 'citizen@example.com')", 
                hashedPassword);

            // Insert Collector
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""User"" (fullname, email, phone, password, role, status, emailverified)
                  SELECT 'Collector', 'collector@example.com', '0900000003', {0}, 'collector', true, true
                  WHERE NOT EXISTS (SELECT 1 FROM ""User"" WHERE email = 'collector@example.com')", 
                hashedPassword);

            // Insert Manager
            await context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""User"" (fullname, email, phone, password, role, status, emailverified)
                  SELECT 'Area Manager', 'manager@example.com', '0900000004', {0}, 'manager', true, true
                  WHERE NOT EXISTS (SELECT 1 FROM ""User"" WHERE email = 'manager@example.com')", 
                hashedPassword);

            // Fetch the inserted IDs and seed related tables
            var citizenIdResult = await context.Users.Where(u => u.Email == "citizen@example.com").Select(u => u.Userid).FirstOrDefaultAsync();
            if (citizenIdResult > 0)
            {
                var hasCitizen = await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM citizen WHERE userid = {0}", citizenIdResult);
                // ExecuteSqlRawAsync returning count is tricky for SELECT, but let's just use raw check
                await context.Database.ExecuteSqlRawAsync("INSERT INTO citizen (userid, totalpoints) SELECT {0}, 0 WHERE NOT EXISTS (SELECT 1 FROM citizen WHERE userid = {0})", citizenIdResult);
            }

            var collectorIdResult = await context.Users.Where(u => u.Email == "collector@example.com").Select(u => u.Userid).FirstOrDefaultAsync();
            var firstTeamId = await context.Teams.Select(t => t.TeamId).FirstOrDefaultAsync();
            if (collectorIdResult > 0 && firstTeamId > 0)
            {
                await context.Database.ExecuteSqlRawAsync("INSERT INTO collector (userid, teamid, status, currenttaskcount) SELECT {0}, {1}, true, 0 WHERE NOT EXISTS (SELECT 1 FROM collector WHERE userid = {0})", collectorIdResult, firstTeamId);
            }

            Console.WriteLine("✅ Sample Users (Admin, Citizen, Staff, Manager) data seeded safely");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  User seed failed: {ex.InnerException?.Message ?? ex.Message}");
    }
}

// Configure the HTTP request pipeline
// Enable Swagger in all environments (for development/testing purposes)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waste Collection Platform API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

// Use Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Disable HTTPS redirect to allow frontend HTTP calls
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
