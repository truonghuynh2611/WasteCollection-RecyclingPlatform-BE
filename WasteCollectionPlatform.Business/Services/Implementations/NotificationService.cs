using Microsoft.Extensions.Logging;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICollectorRepository _collectorRepo;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepo,
        IUserRepository userRepo,
        ICollectorRepository collectorRepo,
        IRealtimeNotifier realtimeNotifier,
        ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _userRepo = userRepo;
        _collectorRepo = collectorRepo;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    public async Task SendNotificationAsync(int userId, string message, int? reportId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            ReportId = reportId,
            CreatedAt = DateTime.Now,
            Isread = false
        };

        await _notificationRepo.AddAsync(notification);
        await _notificationRepo.SaveChangesAsync();

        // Push real-time notification via SignalR
        await _realtimeNotifier.SendToUserAsync(userId, new
        {
            notification.NotificationId,
            notification.Message,
            notification.ReportId,
            notification.CreatedAt,
            IsRead = false
        });
    }

    public async Task SendNotificationToRoleAsync(UserRole role, string message, int? reportId = null)
    {
        _logger.LogInformation("Sending notification to role {Role}: {Message}", role, message);
        
        // Match both true and null (active), but NOT false (inactive)
        var users = await _userRepo.FindAsync(u => u.Role == role && (u.Status == true || u.Status == null));

        if (!users.Any())
        {
            _logger.LogWarning("No active users found with role {Role}", role);
            return;
        }

        foreach (var user in users)
        {
            await SendNotificationAsync(user.UserId, message, reportId);
        }
    }

    public async Task SendNotificationToTeamAsync(int teamId, string message, int? reportId = null)
    {
        var collectors = await _collectorRepo.FindAsync(c => c.TeamId == teamId && c.Status == true);

        foreach (var collector in collectors)
        {
            await SendNotificationAsync(collector.UserId, message, reportId);
        }
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _notificationRepo.GetByUserIdAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepo.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _notificationRepo.MarkAsReadAsync(notificationId);
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _notificationRepo.MarkAllAsReadAsync(userId);
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId)
    {
        await _notificationRepo.DeleteAsync(notificationId);
        await _notificationRepo.SaveChangesAsync();
    }

    public async Task DeleteAllByUserIdAsync(int userId)
    {
        await _notificationRepo.DeleteAllByUserIdAsync(userId);
        await _notificationRepo.SaveChangesAsync();
    }
}
