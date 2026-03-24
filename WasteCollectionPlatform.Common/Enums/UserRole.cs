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
    /// Enterprise/company that manages waste collection
    /// </summary>
    Enterprise = 2,
    
    /// <summary>
    /// System administrator
    /// </summary>
    Admin = 3
}
