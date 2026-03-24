using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task SaveChangesAsync();
    Task DeleteAsync(int notificationId);
    Task DeleteAllByUserIdAsync(int userId);
}
