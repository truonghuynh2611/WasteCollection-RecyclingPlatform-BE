namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Status of a waste report in the workflow
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Report submitted, waiting for enterprise acceptance
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Report accepted by enterprise
    /// </summary>
    Accepted = 1,
    
    /// <summary>
    /// Report assigned to a collector
    /// </summary>
    Assigned = 2,
    
    /// <summary>
    /// Collector is on the way to collect
    /// </summary>
    OnTheWay = 3,
    
    /// <summary>
    /// Waste successfully collected
    /// </summary>
    Collected = 4,
    
    /// <summary>
    /// Collection failed or cancelled
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Report cancelled by citizen
    /// </summary>
    Cancelled = 6
}
