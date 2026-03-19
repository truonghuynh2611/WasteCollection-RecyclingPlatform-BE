using Microsoft.AspNetCore.SignalR;
using WasteCollectionPlatform.API.Hubs;
using WasteCollectionPlatform.Business.Services.Interfaces;

namespace WasteCollectionPlatform.API.Services;

/// <summary>
/// SignalR implementation of IRealtimeNotifier
/// </summary>
public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(int userId, object notification)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }
}
