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

// Tạo DataSource và map enum với name translator để giữ nguyên case
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString!);
dataSourceBuilder.MapEnum<UserRole>("user_role", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());

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
    
    // Seed Area data
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO area (districtid, name) 
            SELECT districtid, v.name
            FROM district d, (VALUES
                ('Quận 1', 'Khu vực 1A'),
                ('Quận 1', 'Khu vực 1B'),
                ('Quận 2', 'Khu vực 2A'),
                ('Quận 2', 'Khu vực 2B'),
                ('Quận 3', 'Khu vực 3A'),
                ('Quận 4', 'Khu vực 4A'),
                ('Quận 5', 'Khu vực 5A'),
                ('Quận 6', 'Khu vực 6A'),
                ('Quận 7', 'Khu vực 7A'),
                ('Quận 7', 'Khu vực 7B'),
                ('Quận 8', 'Khu vực 8A'),
                ('Quận 9', 'Khu vực 9A'),
                ('Quận 10', 'Khu vực 10A'),
                ('Quận 11', 'Khu vực 11A'),
                ('Quận 12', 'Khu vực 12A')
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
            INSERT INTO team (areaid, name, teamtype) 
            SELECT a.areaid, v.name, v.teamtype::team_type
            FROM area a
            INNER JOIN district d ON a.districtid = d.districtid, (VALUES
                ('Khu vực 1A', 'Team Alpha', 'Main'),
                ('Khu vực 1A', 'Team Beta', 'Support'),
                ('Khu vực 1B', 'Team Gamma', 'Main'),
                ('Khu vực 2A', 'Team Delta', 'Main'),
                ('Khu vực 2A', 'Team Epsilon', 'Support'),
                ('Khu vực 2B', 'Team Zeta', 'Main'),
                ('Khu vực 3A', 'Team Eta', 'Main'),
                ('Khu vực 4A', 'Team Theta', 'Main'),
                ('Khu vực 7A', 'Team North', 'Main'),
                ('Khu vực 7B', 'Team South', 'Main'),
                ('Khu vực 9A', 'Team East', 'Support'),
                ('Khu vực 12A', 'Team West', 'Main')
            ) AS v(areaname, name, teamtype)
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
