namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Defines user account status in the system
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Account is active and can use the system
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Account is inactive (deactivated by user or admin)
    /// </summary>
    Inactive = 1,
    
    /// <summary>
    /// Account is pending approval (mainly for Enterprise users)
    /// </summary>
    Pending = 2,
    
    /// <summary>
    /// Account has been suspended by admin
    /// </summary>
    Suspended = 3
}
