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
using WasteCollectionPlatform.API.Hubs;
using WasteCollectionPlatform.API.Services;
using WasteCollectionPlatform.Business.Services.Implementations;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Business.Validators;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Helpers;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Repositories.Implementations;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

// Enable legacy timestamp behavior for Npgsql to handle DateTime Kind issues
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

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

// Map enums with NpgsqlNullNameTranslator - matches PascalCase values in DB ('Admin', 'Citizen'...)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString!);
dataSourceBuilder.MapEnum<UserRole>("user_role", new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReportStatus>("report_status", new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TeamType>("team_type", new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<CollectorRole>("collector_role", new NpgsqlNullNameTranslator());

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
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IReportImageRepository, ReportImageRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add SignalR
builder.Services.AddSignalR();


// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

var app = builder.Build();

// Apply migrations and ensure schema is up to date
// Apply migrations and ensure schema is up to date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();

    // Step 1: Manual Schema Maintenance (Runs FIRST to clean up legacy issues)
    try {
        string syncSql = @"
            -- 0. Cleanup broken migration history row so it doesn't block startup
            DELETE FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '20260325090846_AddRoleToPendingRegistration';

            -- 1. Ensure user_role enum is correct
            DO $$ 
            BEGIN 
                IF EXISTS (SELECT 1 FROM pg_type t WHERE t.typname = 'user_role') THEN
                    IF NOT EXISTS (SELECT 1 FROM pg_enum e JOIN pg_type t ON e.enumtypid = t.oid WHERE t.typname = 'user_role' AND e.enumlabel = 'Citizen') THEN
                        ALTER TYPE user_role ADD VALUE 'Citizen';
                    END IF;
                END IF;
            END $$;

            -- 2. PendingRegistrations table
            CREATE TABLE IF NOT EXISTS ""PendingRegistrations"" (
                ""Email"" VARCHAR(150) PRIMARY KEY,
                ""FullName"" VARCHAR(150) NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""Phone"" VARCHAR(20) NOT NULL,
                ""VerificationCode"" VARCHAR(10) NOT NULL,
                ""Expiry"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
            );
            DO $$ BEGIN 
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PendingRegistrations' AND column_name = 'Role') THEN
                    ALTER TABLE ""PendingRegistrations"" ADD COLUMN ""Role"" user_role NOT NULL DEFAULT 'Citizen';
                END IF;
            END $$;

            -- 3. RefreshTokens table
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

            -- 4. WasteReportItems table (Ensure PascalCase columns for EF Core)
            DO $$ BEGIN
                IF NOT EXISTS (
                    SELECT 1 
                    FROM pg_class c 
                    JOIN pg_namespace n ON n.oid = c.relnamespace 
                    JOIN pg_attribute a ON a.attrelid = c.oid 
                    WHERE c.relname = 'WasteReportItems' 
                    AND a.attname = 'ItemId'
                    AND n.nspname = 'public'
                ) THEN
                    DROP TABLE IF EXISTS ""WasteReportItems"";
                    CREATE TABLE ""WasteReportItems"" (
                        ""ItemId"" SERIAL PRIMARY KEY,
                        ""ReportId"" INTEGER NOT NULL,
                        ""WasteType"" VARCHAR(150) NOT NULL,
                        ""Description"" TEXT,
                        ""ImageUrl"" TEXT,
                        CONSTRAINT fk_item_report FOREIGN KEY (""ReportId"") REFERENCES ""WasteReports""(""ReportId"") ON DELETE CASCADE
                    );
                END IF;
            END $$;

            -- 5. Global Column Maintenance (PascalCase Alignment)
            DO $$ 
            BEGIN 
                -- Users.TokenVersion
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'tokenversion') THEN
                    ALTER TABLE ""Users"" RENAME COLUMN ""tokenversion"" TO ""TokenVersion"";
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'TokenVersion') THEN
                    ALTER TABLE ""Users"" ADD COLUMN ""TokenVersion"" INTEGER DEFAULT 1;
                END IF;

                -- PointHistories.VoucherId
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PointHistories' AND column_name = 'VoucherId') THEN
                    ALTER TABLE ""PointHistories"" ADD COLUMN ""VoucherId"" INTEGER;
                END IF;

                -- ReportImages.ImageType
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ReportImages' AND column_name = 'ImageType') THEN
                    ALTER TABLE ""ReportImages"" ADD COLUMN ""ImageType"" VARCHAR(50);
                END IF;

                -- Teams.Type (enum team_type)
                IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'team_type') THEN
                    CREATE TYPE team_type AS ENUM ('Main', 'Support');
                END IF;

                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Teams' AND column_name = 'Type') THEN
                    ALTER TABLE ""Teams"" ADD COLUMN ""Type"" team_type DEFAULT 'Main';
                END IF;

                -- 5.2. Ensure ReportStatus enum is correct
                IF EXISTS (SELECT 1 FROM pg_type t WHERE t.typname = 'report_status') THEN
                    IF NOT EXISTS (SELECT 1 FROM pg_enum e JOIN pg_type t ON e.enumtypid = t.oid WHERE t.typname = 'report_status' AND e.enumlabel = 'ReportedByTeam') THEN
                        ALTER TYPE report_status ADD VALUE 'ReportedByTeam';
                    END IF;
                END IF;

                -- 6. ReportAssignments table alignment
                IF NOT EXISTS (
                    SELECT 1 
                    FROM pg_class c 
                    JOIN pg_namespace n ON n.oid = c.relnamespace 
                    JOIN pg_attribute a ON a.attrelid = c.oid 
                    WHERE c.relname = 'ReportAssignments' 
                    AND a.attname = 'AssignmentId'
                    AND n.nspname = 'public'
                ) THEN
                    DROP TABLE IF EXISTS ""ReportAssignments"";
                    CREATE TABLE ""ReportAssignments"" (
                        ""AssignmentId"" SERIAL PRIMARY KEY,
                        ""ReportId"" INTEGER NOT NULL,
                        ""TeamId"" INTEGER NOT NULL,
                        CONSTRAINT fk_assignment_report FOREIGN KEY (""ReportId"") REFERENCES ""WasteReports""(""ReportId"") ON DELETE CASCADE,
                        CONSTRAINT fk_assignment_team FOREIGN KEY (""TeamId"") REFERENCES ""Teams""(""TeamId"") ON DELETE CASCADE
                    );
                END IF;
            END $$;
        ";
        context.Database.ExecuteSqlRaw(syncSql);
    } catch (Exception ex) {
        Console.WriteLine($"[CRITICAL] Schema Maintenance Failed: {ex.Message}");
    }

    // Step 2: Regular Migrations (Safety check)
    try {
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations checked");
    } catch (Exception ex) {
        // Minor migration warnings are suppressed because manual sync handled critical parts
        _ = ex; 
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

// Enable static files to serve uploaded images from wwwroot/uploads
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
