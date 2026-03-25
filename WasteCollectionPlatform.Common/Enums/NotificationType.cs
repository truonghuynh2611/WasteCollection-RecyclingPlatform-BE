namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Types of notifications in the system
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Report status change notification
    /// </summary>
    ReportStatusChange = 0,
    
    /// <summary>
    /// New report assigned to collector
    /// </summary>
    ReportAssigned = 1,
    
    /// <summary>
    /// Points earned notification
    /// </summary>
    PointsEarned = 2,
    
    /// <summary>
    /// Voucher redeemed notification
    /// </summary>
    VoucherRedeemed = 3,
    
    /// <summary>
    /// General system notification
    /// </summary>
    System = 4,
    
    /// <summary>
    /// Account status change
    /// </summary>
    AccountStatus = 5
}
