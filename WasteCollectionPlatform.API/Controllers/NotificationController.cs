using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;
    private readonly IUserRepository _userRepo;

    public NotificationController(
        INotificationService notificationService,
        IUserRepository userRepo,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _userRepo = userRepo;
        _logger = logger;
    }

    [HttpGet("test-admins")]
    public async Task<IActionResult> TestAdmins()
    {
        var admins = await _userRepo.FindAsync(u => u.Role == UserRole.Admin && (u.Status == true || u.Status == null));
        return Ok(admins.Select(u => new { u.UserId, u.Email, u.Role, u.Status }));
    }

    [HttpPost("test-insert")]
    public async Task<IActionResult> TestInsertNotification([FromQuery] int userId = 5)
    {
        try
        {
            await _notificationService.SendNotificationAsync(userId, "Test notification from API debug");
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { success = true, message = "Notification inserted!", unreadCount = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message, inner = ex.InnerException?.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get all notifications for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        try
        {
            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get unread notification count for a user
    /// </summary>
    [HttpGet("user/{userId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(int userId)
    {
        try
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Mark a single notification as read
    /// </summary>
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok("Notification marked as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {Id} as read", notificationId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    [HttpPut("user/{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(int userId)
    {
        try
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok("All notifications marked as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a single notification
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> Delete(int notificationId)
    {
        try
        {
            await _notificationService.DeleteAsync(notificationId);
            return Ok("Notification deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {Id}", notificationId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete all notifications for a user
    /// </summary>
    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteAll(int userId)
    {
        try
        {
            await _notificationService.DeleteAllByUserIdAsync(userId);
            return Ok("All notifications deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all notifications for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
    }
}
