using WasteCollectionPlatform.Common.DTOs.Response.Dashboard;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardStatsAsync();
    Task<CollectorDashboardDto> GetCollectorDashboardStatsAsync(int userId);
}
