namespace WasteCollectionPlatform.Common.Constants;

/// <summary>
/// Application settings constants
/// </summary>
public static class AppSettings
{
    // JWT Settings Keys
    public const string JwtSecret = "JwtSettings:Secret";
    public const string JwtIssuer = "JwtSettings:Issuer";
    public const string JwtAudience = "JwtSettings:Audience";
    public const string JwtExpirationMinutes = "JwtSettings:ExpirationMinutes";
    
    // Database Settings
    public const string DefaultConnection = "ConnectionStrings:DefaultConnection";
    
    // Application Settings
    public const string AppName = "Waste Collection Platform";
    public const string ApiVersion = "v1";
    
    // Pagination
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    
    // Password Requirements
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 100;
    
    // BCrypt Work Factor
    public const int BcryptWorkFactor = 12;
}
