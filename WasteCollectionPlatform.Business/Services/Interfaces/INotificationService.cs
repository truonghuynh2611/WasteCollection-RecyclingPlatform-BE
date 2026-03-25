using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(int userId, string message, int? reportId = null);
    Task SendNotificationToRoleAsync(UserRole role, string message, int? reportId = null);
    Task SendNotificationToTeamAsync(int teamId, string message, int? reportId = null);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteAsync(int notificationId);
    Task DeleteAllByUserIdAsync(int userId);
}
