using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly WasteManagementContext _context;

    public NotificationController(WasteManagementContext context)
    {
        _context = context;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(int userId)
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == userId && (n.Isread == false || n.Isread == null));
        return Ok(count);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.Isread = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("user/{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && (n.Isread == false || n.Isread == null))
            .ToListAsync();

        foreach (var n in notifications)
        {
            n.Isread = true;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteAllNotifications(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
