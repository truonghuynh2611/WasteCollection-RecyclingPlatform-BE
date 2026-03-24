using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && (n.Isread == null || n.Isread == false));
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.Isread = true;
            _context.Notifications.Update(notification);
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && (n.Isread == null || n.Isread == false))
            .ToListAsync();

        foreach (var n in unread)
        {
            n.Isread = true;
        }

        _context.Notifications.UpdateRange(unread);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId)
    {
        await _context.Notifications
            .Where(n => n.NotificationId == notificationId)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteAllByUserIdAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId)
            .ExecuteDeleteAsync();
    }
}
