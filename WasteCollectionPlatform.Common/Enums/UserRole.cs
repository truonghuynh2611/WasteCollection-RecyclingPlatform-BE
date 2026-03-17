namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Defines user roles in the system
/// Maps to PostgreSQL ENUM user_role with PascalCase values
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular citizen who reports waste
    /// </summary>
    Citizen = 0,
    
    /// <summary>
    /// Collector who works for a team
    /// </summary>
    Collector = 1,
    
    /// <summary>
    /// Legacy role to prevent crash - DO NOT USE
    /// </summary>
    Enterprise = 2,
    
    /// <summary>
    /// System administrator
    /// </summary>
    Admin = 3,
    
    /// <summary>
    /// Legacy role to prevent crash - DO NOT USE
    /// </summary>
    Manager = 4
}
