namespace WasteCollectionPlatform.Common.Enums;

/// <summary>
/// Status of a waste report in the workflow.
/// Values must match the PostgreSQL report_status enum exactly.
/// </summary>
public enum ReportStatus
{
    /// <summary>Report submitted, waiting for enterprise acceptance</summary>
    Pending = 0,

    /// <summary>Report accepted by enterprise</summary>
    Accepted = 1,

    /// <summary>Report assigned to a collector team</summary>
    Assigned = 2,

    /// <summary>Collector is on the way to collect</summary>
    OnTheWay = 3,

    /// <summary>Waste successfully collected – citizen earns points</summary>
    Collected = 4,

    /// <summary>Report failed / cancelled – citizen does not earn points</summary>
    Failed = 5
}
