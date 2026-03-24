namespace WasteCollectionPlatform.Business.Services.Interfaces;

/// <summary>
/// Abstraction for real-time notification push (implemented by SignalR in API layer)
/// </summary>
public interface IRealtimeNotifier
{
    Task SendToUserAsync(int userId, object notification);
}
