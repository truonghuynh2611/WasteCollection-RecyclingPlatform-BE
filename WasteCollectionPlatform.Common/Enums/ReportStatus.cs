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
    Assigned = 1,
    
    /// <summary>
    /// Report assigned to a collector
    /// </summary>
    Processing = 2,
    
    /// <summary>
    /// Collector is on the way to collect
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// Waste successfully collected
    /// </summary>
    Cancelled = 4
}
