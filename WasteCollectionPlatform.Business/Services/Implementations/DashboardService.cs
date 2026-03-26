using Microsoft.Extensions.Logging;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Response.Dashboard;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardStatsAsync()
    {
        var allReports = await _unitOfWork.WasteReports.GetAllAsync();
        var allCitizens = await _unitOfWork.Citizens.GetAllAsync();
        var allTeams = await _unitOfWork.Teams.GetAllAsync();
        var allPointHistories = await _unitOfWork.PointHistories.GetAllAsync();

        var dto = new AdminDashboardDto
        {
            TotalReports = allReports.Count(),
            PendingReports = allReports.Count(r => r.Status == ReportStatus.Pending),
            ProcessingReports = allReports.Count(r => r.Status == ReportStatus.Accepted || r.Status == ReportStatus.Assigned || r.Status == ReportStatus.OnTheWay),
            CompletedReports = allReports.Count(r => r.Status == ReportStatus.Collected),
            TotalCitizens = allCitizens.Count(),
            ActiveTeams = allTeams.Count(),
            TotalPointsAwarded = allPointHistories.Where(ph => ph.PointAmount > 0).Sum(ph => (decimal)ph.PointAmount),
            RecentActivities = allReports
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentActivityDto
                {
                    ReportId = r.ReportId,
                    Message = GetAdminActivityMessage(r),
                    Timestamp = r.CreatedAt ?? DateTime.Now,
                    Status = r.Status.ToString()
                }).ToList()
        };

        return dto;
    }

    public async Task<CollectorDashboardDto> GetCollectorDashboardStatsAsync(int userId)
    {
        var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
        if (collector == null) 
        {
            _logger.LogWarning("GetCollectorDashboardStats: User {UserId} is not a collector (likely Admin)", userId);
            return new CollectorDashboardDto 
            { 
                TeamName = "N/A (Admin View)", 
                RecentTasks = new List<RecentTaskDto>() 
            };
        }

        var teamId = collector.TeamId;
        var allReports = await _unitOfWork.WasteReports.GetAllAsync();
        var teamReports = teamId.HasValue ? allReports.Where(r => r.TeamId == teamId.Value).ToList() : new List<WasteReport>();
        
        var team = teamId.HasValue ? await _unitOfWork.Teams.GetByIdAsync(teamId.Value) : null;

        var dto = new CollectorDashboardDto
        {
            TeamName = team?.Name ?? "N/A",
            MyAssignedTasks = teamReports.Count(r => (r.Status == ReportStatus.Assigned || r.Status == ReportStatus.OnTheWay)),
            TeamCompletedToday = teamReports.Count(r => r.Status == ReportStatus.Collected && (r.CreatedAt ?? DateTime.MinValue).Date == DateTime.Today),
            TeamTotalCompleted = teamReports.Count(r => r.Status == ReportStatus.Collected),
            RecentTasks = teamReports
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentTaskDto
                {
                    ReportId = r.ReportId,
                    Address = r.Area?.Name ?? "Chưa xác định",
                    WasteType = r.WasteType ?? "Tổng hợp",
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt ?? DateTime.Now
                }).ToList()
        };

        return dto;
    }

    private string GetAdminActivityMessage(WasteReport report)
    {
        return report.Status switch
        {
            ReportStatus.Pending => $"Yêu cầu mới #{report.ReportId} được tạo.",
            ReportStatus.Assigned => $"Nhiệm vụ #{report.ReportId} đã được gán cho Đội #{report.TeamId}.",
            ReportStatus.OnTheWay => $"Đội #{report.TeamId} đang thu gom nhiệm vụ #{report.ReportId}.",
            ReportStatus.Collected => $"Nhiệm vụ #{report.ReportId} đã hoàn thành.",
            ReportStatus.Failed => $"Nhiệm vụ #{report.ReportId} thất bại/bị hủy.",
            _ => $"Báo cáo #{report.ReportId} thay đổi trạng thái sang {report.Status}."
        };
    }
}
