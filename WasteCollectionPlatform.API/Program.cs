using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Npgsql.NameTranslation;
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
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

// Map enum với NpgsqlNullNameTranslator - không convert case (DB có PascalCase: 'Admin', 'Citizen'...)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString!);
dataSourceBuilder.MapEnum<UserRole>(
    "user_role",
    new NpgsqlNullNameTranslator()
);

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
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IReportImageRepository, ReportImageRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();
// Add other services here as they are implemented

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

var app = builder.Build();

// ⚠️ ALL MIGRATIONS AND SEED CODE COMMENTED OUT - Using PascalCase tables now
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();
    
    // RefreshToken table migration
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""RefreshTokens"" (
                ""RefreshTokenId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL,
                ""Token"" VARCHAR(500) NOT NULL UNIQUE,
                ""ExpiresAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""IsRevoked"" BOOLEAN DEFAULT FALSE,
                ""RevokedAt"" TIMESTAMP WITHOUT TIME ZONE,
                CONSTRAINT fk_refreshtoken_user FOREIGN KEY (""UserId"") REFERENCES ""Users""(""UserId"") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS idx_refreshtoken_token ON ""RefreshTokens""(""Token"");
            CREATE INDEX IF NOT EXISTS idx_refreshtoken_userid ON ""RefreshTokens""(""UserId"");
        ");
        Console.WriteLine("✅ RefreshToken table migration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  RefreshToken table already exists or migration failed: {ex.Message}");
    }
    
    // Email Verification migration
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""Users"" 
            ADD COLUMN IF NOT EXISTS ""EmailVerified"" BOOLEAN DEFAULT FALSE,
            ADD COLUMN IF NOT EXISTS ""VerificationToken"" VARCHAR(500),
            ADD COLUMN IF NOT EXISTS ""VerificationTokenExpiry"" TIMESTAMP WITHOUT TIME ZONE,
            ADD COLUMN IF NOT EXISTS ""ResetPasswordToken"" VARCHAR(500),
            ADD COLUMN IF NOT EXISTS ""ResetTokenExpiry"" TIMESTAMP WITHOUT TIME ZONE;
            
            CREATE INDEX IF NOT EXISTS idx_users_verificationtoken ON ""Users""(""VerificationToken"") WHERE ""VerificationToken"" IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_users_resetpasswordtoken ON ""Users""(""ResetPasswordToken"") WHERE ""ResetPasswordToken"" IS NOT NULL;
        ");
        Console.WriteLine("✅ Email verification and password reset fields migration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Email verification fields already exist or migration failed: {ex.Message}");
    }
    
    // Enterprise table migration
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Enterprises"" (
                ""EnterpriseId"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL UNIQUE,
                ""DistrictId"" INTEGER,
                ""WasteTypes"" VARCHAR(255),
                ""DailyCapacity"" INTEGER,
                ""CurrentLoad"" INTEGER DEFAULT 0,
                ""Status"" BOOLEAN DEFAULT TRUE,
                CONSTRAINT fk_enterprise_user FOREIGN KEY (""UserId"") REFERENCES ""Users""(""UserId"") ON DELETE CASCADE,
                CONSTRAINT fk_enterprise_district FOREIGN KEY (""DistrictId"") REFERENCES ""Districts""(""DistrictId"") ON DELETE SET NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS enterprise_userid_key ON ""Enterprises""(""UserId"");
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
            INSERT INTO ""Districts"" (""DistrictName"") 
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
                SELECT 1 FROM ""Districts"" WHERE ""DistrictName"" = v.districtname
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
            INSERT INTO ""Areas"" (""DistrictId"", ""Name"") 
            SELECT d.""DistrictId"", v.name
            FROM ""Districts"" d, (VALUES
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
            WHERE d.""DistrictName"" = v.districtname
            AND NOT EXISTS (
                SELECT 1 FROM ""Areas"" WHERE ""DistrictId"" = d.""DistrictId"" AND ""Name"" = v.name
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
            INSERT INTO ""Teams"" (""AreaId"", ""Name"") 
            SELECT a.""AreaId"", v.name
            FROM ""Areas"" a
            INNER JOIN ""Districts"" d ON a.""DistrictId"" = d.""DistrictId"", (VALUES
                ('Phường Bến Nghé', 'Team Alpha'),
                ('Phường Bến Nghé', 'Team Beta'),
                ('Phường Bến Thành', 'Team Gamma'),
                ('Phường 1', 'Team Delta'),
                ('Phường 1', 'Team Epsilon'),
                ('Phường 2', 'Team Zeta'),
                ('Võ Thị Sáu', 'Team Eta'),
                ('Phường 1', 'Team Theta'),
                ('Phường 1', 'Team North'),
                ('Phường 2', 'Team South'),
                ('Phường 1', 'Team East'),
                ('Phường 1', 'Team West')
            ) AS v(areaname, name)
            WHERE a.""Name"" = v.areaname
            AND NOT EXISTS (
                SELECT 1 FROM ""Teams"" WHERE ""AreaId"" = a.""AreaId"" AND ""Name"" = v.name
            );
        ");
        Console.WriteLine("✅ Team data seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Team seed failed (might already exist): {ex.Message}");
    }
}
*/

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
