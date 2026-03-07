namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Defines user roles in the system
/// ⚠ IMPORTANT: Order must match PostgreSQL ENUM: ('Citizen', 'Collector', 'Enterprise', 'Admin')
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
<<<<<<< HEAD
    Admin = 3,

    /// <summary>
    /// Area manager handling staffs and schedules
    /// </summary>
    Manager = 4
=======
    Admin = 3
>>>>>>> 7c8b4ebc26e3f329a9474e114f446ddad48d1530
}
