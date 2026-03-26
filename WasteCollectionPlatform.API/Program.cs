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

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.WebHost.UseUrls("http://0.0.0.0:61436");

// Set default culture to Invariant to ensure consistent decimal parsing
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

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
dataSourceBuilder.MapEnum<CollectorRole>("collector_role", new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReportStatus>("report_status", new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TeamType>("team_type", new NpgsqlNullNameTranslator());

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<WasteManagementContext>(options =>
    options.UseNpgsql(dataSource)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

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
        policy.SetIsOriginAllowed(_ => true) // Allow any origin for development
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
builder.Services.AddScoped<IDistrictService, DistrictService>();

// Add SignalR
builder.Services.AddSignalR();


// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

var app = builder.Build();

// Apply migrations and ensure schema is up to date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();
    try
    {
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration error: {ex.Message}");
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();
    try {
        string sql = @"
            CREATE TABLE IF NOT EXISTS ""PendingRegistrations"" (
                ""Email"" VARCHAR(150) PRIMARY KEY,
                ""FullName"" VARCHAR(150) NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""Phone"" VARCHAR(20) NOT NULL,
                ""VerificationCode"" VARCHAR(10) NOT NULL,
                ""Expiry"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
            );";
        context.Database.ExecuteSqlRaw(sql);
    } catch {}

    try {
        // Ensure VoucherId exists in PointHistories (missed in previous manual migrations)
        string alterSql = @"
            ALTER TABLE ""PointHistories"" 
            ADD COLUMN ""VoucherId"" integer NULL;
            
            ALTER TABLE ""PointHistories""
            ADD CONSTRAINT fk_point_voucher FOREIGN KEY (""VoucherId"") REFERENCES ""Vouchers"" (""VoucherId"") ON DELETE SET NULL;
        ";
        context.Database.ExecuteSqlRaw(alterSql);

        // Robustly ensure TokenVersion exists with correct casing (PascalCase)
    } catch (Exception ex) {
        // Ignored if column already exists
        Console.WriteLine($"PointHistories Migration note (safe to ignore): {ex.Message}");
    }

    try {
        string updateSql = @"
            ALTER TABLE ""WasteReports"" ADD COLUMN IF NOT EXISTS ""Note"" TEXT;
            ALTER TABLE ""Collectors"" ALTER COLUMN ""TeamId"" DROP NOT NULL;
        ";
        context.Database.ExecuteSqlRaw(updateSql);
        
        // Add new enum values to report_status if they don't exist
        var enums = new[] { "Accepted", "OnTheWay", "Collected", "Failed" };
        foreach (var val in enums)
        {
            try {
                context.Database.ExecuteSqlRaw($@"ALTER TYPE report_status ADD VALUE '{val}';");
            } catch { /* Ignore if it already exists */ }
        }
    } catch (Exception ex) {
        Console.WriteLine($"WasteReport/Collector Migration note: {ex.Message}");
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

app.UseStaticFiles();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
