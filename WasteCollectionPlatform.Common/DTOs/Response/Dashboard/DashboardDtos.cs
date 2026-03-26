using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Response.Dashboard;

public class AdminDashboardDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; } // Status 0
    public int ProcessingReports { get; set; } // Status 1, 2, 3
    public int CompletedReports { get; set; } // Status 5
    public int TotalCitizens { get; set; }
    public int ActiveTeams { get; set; }
    public decimal TotalPointsAwarded { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class CollectorDashboardDto
{
    public int MyAssignedTasks { get; set; }
    public int TeamCompletedToday { get; set; }
    public int TeamTotalCompleted { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public List<RecentTaskDto> RecentTasks { get; set; } = new();
}

public class RecentActivityDto
{
    public int ReportId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RecentTaskDto
{
    public int ReportId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string WasteType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
